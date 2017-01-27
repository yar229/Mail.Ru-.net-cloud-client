using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MailRuCloudApi.Api.Streams.FileInfo;
using MailRuCloudApi.Api.Streams.Splitters;
using MailRuCloudApi.Api.Streams.Transformers;
using Newtonsoft.Json;

namespace MailRuCloudApi.Api.Streams
{
    class SplittedUploadStream : Stream
    {
        class FileContainer
        {
            public File File;
            public IByteTransformer Transformer;
        }


        private readonly CloudApi _cloud;
        private long _size;
        private readonly Func<IByteTransformer> _transformerFunc;
        private readonly File _origfile;

        private int _currFileId = -1;
        protected long BytesWrote;
        private UploadStream _uploadStream;


        private readonly List<FileContainer> _files;

        public SplittedUploadStream(string destinationPath, CloudApi cloud, long size, IFileSplitter fileSplitter, Func<IByteTransformer> transformerFuncFunc)
        {
            _cloud = cloud;
            _size = size;
            _transformerFunc = transformerFuncFunc;

            _origfile = new File(destinationPath, _size, null);
            _files = fileSplitter
                .SplitFile(_origfile)
                .Select(f => new FileContainer
                {
                    File = f,
                    Transformer = transformerFuncFunc?.Invoke()

                }).ToList();

            NextFile();
        }


        private void NextFile()
        {
            if (_currFileId >= 0)
                _uploadStream.Close();

            _currFileId++;
            if (_currFileId >= _files.Count)
                return;

            BytesWrote = 0;
            _uploadStream = new UploadStream(_files[_currFileId].File.FullPath, _cloud, _files[_currFileId].File.Size.DefaultValue);
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
            var z = null == _files[_currFileId].Transformer
                ? buffer
                : _files[_currFileId].Transformer.Transform(buffer, offset, count);


            var filefreeleft = _files[_currFileId].File.Size.DefaultValue - BytesWrote;
            if (filefreeleft >= count)
            {
                _uploadStream.Write(z, offset, count);
                BytesWrote += count;
            }
            else
            {
                _uploadStream.Write(z, offset, (int)filefreeleft);
                NextFile();
                Write(z, offset + (int)filefreeleft, count - (int)filefreeleft);
            }
        }

        public override void Close()
        {
            if (_files.Any(f => f.File.Name != _origfile.Name))
            {
                var info = new SplittedFileInfo
                {
                    Name = _origfile.Name,
                    Size = _origfile.Size.DefaultValue,
                    Crc32 = 0, //TODO: calculate CRC32
                    Key = null,
                    Parts = _files
                        .Where(f => f.File.Name != _origfile.Name)
                        .Select(f => new FileInfo.FileInfo
                        {
                            Name = f.File.Name,
                            Size = f.File.Size.DefaultValue,
                            Crc32 = 0,
                            Key = f.Transformer.Data
                        })
                        .ToList()
                };

                string content = JsonConvert.SerializeObject(info);

                var data = Encoding.UTF8.GetBytes(content);
                var stream = new UploadStream(_origfile.FullPath, _cloud, data.Length);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            ////remove test file created with webdav
            //var dele = new RemoveRequest(_cloud, _origfile.FullPath)
            //    .MakeRequestAsync().Result;

            _uploadStream?.Close();
        }


        //uint CalculateCrc(uint crc, byte[] buffer, int offset, int count)
        //{
        //    unchecked
        //    {
        //        for (int i = offset, end = offset + count; i < end; i++)
        //            crc = (crc >> 8) ^ CrcTable[(crc ^ buffer[i]) & 0xFF];
        //    }
        //    return crc;
        //}

        //private static readonly uint[] CrcTable = GenerateTable();

        //private static uint[] GenerateTable()
        //{
        //    unchecked
        //    {
        //        uint[] table = new uint[256];

        //        const uint poly = 0xEDB88320;
        //        for (uint i = 0; i < table.Length; i++)
        //        {
        //            var crc = i;
        //            for (int j = 8; j > 0; j--)
        //            {
        //                if ((crc & 1) == 1) crc = (crc >> 1) ^ poly;
        //                else crc >>= 1;
        //            }
        //            table[i] = crc;
        //        }

        //        return table;
        //    }

        //}


        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _size;
        public override long Position { get; set; }

    }
}
