using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Streams.Transformers;
using Newtonsoft.Json;

namespace MailRuCloudApi.Api.Streams
{
    public class DownloadStream : Stream
    {
        private const int InnerBufferSize = 65536;

        private IList<File> _files => _file.Files;
        private readonly ShardInfo _shard;
        private readonly File _file;
        private readonly CloudApi _cloud;
        private readonly long? _start;
        private readonly long? _end;
        private readonly Func<byte[], IByteTransformer> _transformerFunc;

        private RingBufferedStream _innerStream;
        private FileInfo.SplittedFileInfo _headers;


        public DownloadStream(File file, CloudApi cloud, long? start, long? end, Func<byte[], IByteTransformer> transformerFunc)
        {
            _file = file;
            _cloud = cloud;
            _start = start;
            _end = end;
            _transformerFunc = transformerFunc;

            _shard = _cloud.GetShardInfo(ShardType.Get).Result;

            Initialize();
        }


        private FileInfo.SplittedFileInfo GetHeader(File file, CloudApi api)
        {
            var f = file.IsSplitted
                        ? new File(file.FullPath, file.Size.DefaultValue, "") 
                        : file.Files.First(fi => fi.Name == file.Name);

            var data = api.GetFile(f);
            var str = Encoding.UTF8.GetString(data);
            var obj = JsonConvert.DeserializeObject<FileInfo.SplittedFileInfo>(str);
            return obj;
        }

        private void Initialize()
        {
            if (_file.IsSplitted) _headers = GetHeader(_file, _cloud);

            _innerStream = new RingBufferedStream(InnerBufferSize);

            // ReSharper disable once UnusedVariable
            var t = GetFileStream();
        }



        private async Task<object> GetFileStream()
        {
            foreach (var file in _files)
            {
                var request = (HttpWebRequest)WebRequest.Create($"{_shard.Url}{Uri.EscapeDataString(file.FullPath)}");
                request.Proxy = _cloud.Account.Proxy;
                request.CookieContainer = _cloud.Account.Cookies;
                request.Method = "GET";
                request.ContentType = ConstSettings.DefaultRequestType;
                request.Accept = ConstSettings.DefaultAcceptType;
                request.UserAgent = ConstSettings.UserAgent;
                request.AllowReadStreamBuffering = false;

                var length = file.Size.DefaultValue;
                if (_start != null)
                {
                    var start = _start ?? 0;
                    var end = Math.Min(_end ?? long.MaxValue, length - 1);
                    length = end - start + 1;

                    request.Headers.Add("Content-Range", $"bytes {start}-{end} / {length}");
                }

                var z = _transformerFunc(_headers.Parts.First(hi => hi.Name == file.Name).Key);

                var task = Task.Factory.FromAsync(request.BeginGetResponse,
                    asyncResult => request.EndGetResponse(asyncResult), null);
                await task.ContinueWith(
                    (t, m) =>
                    {
                        var token = (CancellationToken)m;
                        {
                            try
                            {
                                ReadResponseAsByte(t.Result, token, _innerStream, z);
                                return _innerStream;
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        }
                    },
                    _cloud.CancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);

            }

            _innerStream.Flush();
            return _innerStream;
        }


        public override void Close()
        {
            _innerStream.Close();
            base.Close();
        }

        private void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream, IByteTransformer byteTransformer)
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

                    var data = null == byteTransformer 
                        ? buffer
                        : byteTransformer.Transform(buffer, 0, bytesRead);

                    outputStream?.Write(data, 0, bytesRead);
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
