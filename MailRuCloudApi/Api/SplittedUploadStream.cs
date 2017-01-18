using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Requests;

namespace MailRuCloudApi.Api
{
    class SplittedUploadStream : Stream
    {
        private readonly string _destinationPath;
        private readonly CloudApi _cloud;
        private long _size;
        private readonly ShardInfo _shard;
        private readonly long _maxFileSize;
        private File _origfile;

        private int _currFileId = -1;
        private int _bytesWrote = 0;
        private UploadStream _uploadStream;


        private readonly List<File> _files = new List<File>();

        public SplittedUploadStream(string destinationPath, CloudApi cloud, long size)
        {
            _destinationPath = destinationPath;
            _cloud = cloud;
            _size = size;

            _shard = _cloud.GetShardInfo(ShardType.Upload).Result;
            _maxFileSize = _cloud.Account.Info.FileSizeLimit - 1024*1024; //50000; 

            Initialize();
        }

        private void Initialize()
        {
            long allowedSize = _maxFileSize - 1024; //TODO: make it right //- BytesCount(_file.Name);
            _origfile = new File(_destinationPath, _size, null);
            if (_size <= allowedSize)
            {
                _files.Add(_origfile);
            }
            else
            {
                int nfiles = (int) (_size/allowedSize + 1);
                if (nfiles > 999)
                    throw new OverflowException("Cannot upload more than 999 file parts");
                for (int i = 1; i <= nfiles; i++)
                {
                    var f = new File($"{_origfile.FullPath}.wdmrc.{i:D3}",
                        i != nfiles ? allowedSize : _size%allowedSize,
                        null);
                    _files.Add(f);
                }
            }

            NextFile();
        }

        private void NextFile()
        {
            if (_currFileId >= 0)
            {
                _uploadStream.Close();
            }

            _currFileId++;
            if (_currFileId >= _files.Count)
                return;

            _bytesWrote = 0;
            _uploadStream = new UploadStream(_files[_currFileId].FullPath, _cloud, _files[_currFileId].Size.DefaultValue);
        }


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _size = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                long diff = _bytesWrote + count - _files[_currFileId].Size.DefaultValue;

                if (diff > 0)
                {
                    var zbuffer = new byte[buffer.Length];
                    buffer.CopyTo(zbuffer, 0);
                    long zcount = count;

                    _uploadStream.Write(zbuffer, offset, (int)(zcount - diff));

                    NextFile();
                }

                //var z = diff <= 0 ? count : diff;
                long ncount = diff <= 0 ? count : diff;
                var nbuffer = new byte[ncount];
                Array.Copy(buffer, count - ncount, nbuffer, 0, ncount);


                _uploadStream.Write(nbuffer, offset, (int)ncount);

            }
            catch (Exception)
            {

                throw;
            }

        }

        public override void Close()
        {
            if (_files.Count > 1)
            {
                string content = string.Empty;
                var data = Encoding.UTF8.GetBytes(content);
                var stream = new UploadStream(_origfile.FullPath + ".wdmrc.crc", _cloud, data.Length);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }


            _uploadStream?.Close();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _size;
        public override long Position { get; set; }

    }
}
