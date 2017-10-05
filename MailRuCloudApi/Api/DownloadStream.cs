﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Requests;

namespace MailRuCloudApi.Api
{
    public class DownloadStream : Stream
    {
        private const int InnerBufferSize = 65536;

        private readonly IList<File> _files;
        private readonly ShardInfo _shard;
        private readonly CloudApi _cloud;
        private readonly long? _start;
        private readonly long? _end;

        private RingBufferedStream _innerStream;

        public DownloadStream(File file, CloudApi cloud, long? start = null, long? end = null)
            : this(file.Files, cloud, start, end)
        {
        }

        public DownloadStream(IList<File> files, CloudApi cloud, long? start = null, long? end = null)
        {
            var globalLength = files.Sum(f => f.Size);

            _cloud = cloud;
            _shard = files.All(f => string.IsNullOrEmpty(f.PublicLink))
                ? _cloud.GetShardInfo(ShardType.Get).Result
                : _cloud.GetShardInfo(ShardType.WeblinkGet).Result;

            _files = files;
            _start = start;
            _end = end >= globalLength ? globalLength - 1 : end;

            Length = _start != null && _end != null
                ? _end.Value - _start.Value + 1
                : globalLength;

            Initialize();
        }

        private void Initialize()
        {
            _innerStream = new RingBufferedStream(InnerBufferSize);

            // ReSharper disable once UnusedVariable
            var t = GetFileStream();
        }



        private async Task<object> GetFileStream()
        {
            var totalLength = Length;
            long glostart = _start ?? 0;
            long gloend = _end == null || (_start == _end && _end == 0) ? totalLength : _end.Value + 1;

            long fileStart = 0;
            long fileEnd = 0;

            Task<WebResponse> task = Task.FromResult((WebResponse)null);

            foreach (var file in _files)
            {
                var clofile = file;

                fileEnd += clofile.Size;

                if (glostart >= fileEnd || gloend <= fileStart)
                {
                    fileStart += clofile.Size;
                    continue;
                }
                
                var instart = Math.Max(0, glostart - fileStart);
                var inend = gloend - fileStart - 1;

                task = task.ContinueWith(task1 =>
                {
                    
                    WebResponse response;
                    int cnt = 0;
                    while (true)
                    {
                        try
                        {
                            //TODO: refact
                            string downloadkey = string.Empty;
                            if (_shard.Type == ShardType.WeblinkGet)
                            {
                                //var dtres = new DownloadTokenHtmlRequest(_cloud, file.PublicLink).MakeRequestAsync().Result;
                                //downloadkey = dtres.body.token;
                                downloadkey = _cloud.Account.DownloadToken;
                            }

                            var request = _shard.Type == ShardType.Get
                                ? (HttpWebRequest) WebRequest.Create($"{_shard.Url}{Uri.EscapeDataString(file.FullPath)}")
                                : (HttpWebRequest)WebRequest.Create($"{_shard.Url}{new Uri(file.PublicLink).PathAndQuery.Remove(0, "/public".Length)}?key={downloadkey}");


                            request.Headers.Add("Accept-Ranges", "bytes");
                            request.AddRange(instart, inend);
                            request.Proxy = _cloud.Account.Proxy;
                            request.CookieContainer = _cloud.Account.Cookies;
                            request.Method = "GET";
                            request.ContentType =  MediaTypeNames.Application.Octet;
                            request.Accept = "*/*";
                            request.UserAgent = ConstSettings.UserAgent;
                            request.AllowReadStreamBuffering = false;

                            response = request.GetResponse();
                            break;
                        }
                        catch (WebException wex)
                        {
                            if (wex.Status == WebExceptionStatus.ProtocolError)
                            {
                                if (wex.Response is HttpWebResponse wexresp && wexresp.StatusCode == HttpStatusCode.GatewayTimeout && ++cnt <= 3)
                                    continue;
                            }
                            _innerStream.Close();
                            throw;
                        }
                        
                    }

                    using (Stream responseStream = response.GetResponseStream()) //ReadResponseAsByte(response, CancellationToken.None, _innerStream);
                    {
                        responseStream?.CopyTo(_innerStream);
                    }

                    return response;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

                fileStart += file.Size;
            }

            task = task.ContinueWith(task1 =>
            {
                _innerStream.Flush();
                return (WebResponse)null;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            return _innerStream;
        }


        public override void Close()
        {
            _innerStream.Close();
            base.Close();
        }

        //private void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream = null)
        //{
        //    using (Stream responseStream = resp.GetResponseStream())
        //    {
        //        var buffer = new byte[65536];
        //        int bytesRead;
        //        int totalRead = 0;

        //        while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            token.ThrowIfCancellationRequested();

        //            if (totalRead + bytesRead > Length)
        //            {
        //                bytesRead = (int)(Length - totalRead);
        //            }
        //            totalRead += bytesRead;

        //            outputStream?.Write(buffer, 0, bytesRead);
        //        }
        //    }
        //}


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readed = _innerStream.Read(buffer, offset, count);
            return readed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = true;
        public override bool CanWrite { get; } = false;

        public override long Length { get; }

        public override long Position { get; set; }
    }
}
