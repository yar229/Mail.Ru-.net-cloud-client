using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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


        public DownloadStream(IList<File> files, CloudApi cloud, long? start, long? end)
        {
            _files = files;
            _cloud = cloud;
            _start = start;
            _end = end;

            _shard = _cloud.GetShardInfo(ShardType.Get).Result;

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
            var totalLength = _files.Sum(f => f.Size.DefaultValue);
            var glostart = _start ?? 0;
            var gloend = _end ?? totalLength;

            long fileStart = 0;
            long fileEnd = 0;

            Task<WebResponse> task = Task.FromResult((WebResponse)null);

            foreach (var file in _files)
            {
                //TODO: refact
                var request = (HttpWebRequest)WebRequest.Create($"{_shard.Url}{Uri.EscapeDataString(file.FullPath)}");

                fileEnd += file.Size.DefaultValue;

                if (glostart >= fileEnd || gloend <= fileStart)
                {
                    fileStart += file.Size.DefaultValue;
                    continue;
                }

                //var instart = Math.Min(0, glostart - fileStart);
                var instart = Math.Max(0, glostart - fileStart);
                //var instart = Math.Min(0, Math.Abs(glostart - fileStart));
                var inend = Math.Min(file.Size.DefaultValue, gloend - fileStart);

                request.Headers.Add("Accept-Ranges", "bytes");
                request.AddRange(instart, inend);

                fileStart += file.Size.DefaultValue;


                request.Proxy = _cloud.Account.Proxy;
                request.CookieContainer = _cloud.Account.Cookies;
                request.Method = "GET";
                request.ContentType = "application/octet-stream";//ConstSettings.DefaultRequestType;
                request.Accept = "*/*";
                request.UserAgent = ConstSettings.UserAgent;
                request.AllowReadStreamBuffering = false;

                task = task.ContinueWith(task1 => request.GetResponse(), TaskContinuationOptions.OnlyOnRanToCompletion);

                //var task = Task.Factory.FromAsync(request.BeginGetResponse, asyncResult => request.EndGetResponse(asyncResult), null);
                task = task.ContinueWith(
                    (t, m) =>
                    {
                        var token = (CancellationToken)m;
                        {
                            try
                            {
                                ReadResponseAsByte(t.Result, token, _innerStream);
                                return t.Result;
                                //return _innerStream;
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        }
                    },
                    _cloud.CancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);

            }

            task = task.ContinueWith(task1 =>
            {
                _innerStream.Flush();
                return (WebResponse) null;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);


            //_innerStream.Flush();
            return _innerStream;
        }


        public override void Close()
        {
            _innerStream.Close();
            base.Close();
        }

        private void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream = null)
        {
            using (Stream responseStream = resp.GetResponseStream())
            {
                var buffer = new byte[65536];
                int bytesRead;
                int totalRead = 0;

                while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    token.ThrowIfCancellationRequested();

                    if (totalRead + bytesRead > Length)
                    {
                        bytesRead = (int)(Length - totalRead);
                    }
                    totalRead += bytesRead;

                    outputStream?.Write(buffer, 0, bytesRead);
                }
            }
        }


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

        public override long Length
        {
            get
            {
                if (_start != null && _end != null)
                {
                    var l = _end.Value - _start.Value;
                    return l;
                }

                //return _files.Size.DefaultValue;
                return _files.Sum(f => f.Size.DefaultValue);

            }
        }

        public override long Position { get; set; }
    }
}
