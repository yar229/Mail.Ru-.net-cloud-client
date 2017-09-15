﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MailRuCloudApi.Api
{
    class SplittedUploadStream : Stream
    {
        private readonly string _destinationPath;
        private readonly CloudApi _cloud;
        private long _size;
        private readonly long _maxFileSize;
        private File _origfile;

        private int _currFileId = -1;
        private long _bytesWrote;
        private UploadStream _uploadStream;


        private readonly List<File> _files = new List<File>();

        public SplittedUploadStream(string destinationPath, CloudApi cloud, long size)
        {
            _destinationPath = destinationPath;
            _cloud = cloud;
            _size = size;

            _maxFileSize = _cloud.Account.Info.FileSizeLimit > 0
                ? _cloud.Account.Info.FileSizeLimit - 1024
                : long.MaxValue - 1024;

            Initialize();
        }

        private void Initialize()
        {
            long allowedSize = _maxFileSize; //TODO: make it right //- BytesCount(_file.Name);
            _origfile = new File(_destinationPath, _size, null);
            if (_size <= allowedSize)
            {
                _files.Add(_origfile);
            }
            else
            {
                int nfiles = (int)(_size / allowedSize + 1);
                if (nfiles > 999)
                    throw new OverflowException("Cannot upload more than 999 file parts");
                for (int i = 1; i <= nfiles; i++)
                {
                    var f = new File($"{_origfile.FullPath}.wdmrc.{i:D3}",
                        i != nfiles ? allowedSize : _size % allowedSize,
                        null);
                    _files.Add(f);
                }
            }

            NextFile();
        }

        private void NextFile()
        {
            if (_currFileId >= 0)
                _uploadStream.Close();

            _currFileId++;
            if (_currFileId >= _files.Count)
                return;

            _bytesWrote = 0;
            var currFile = _files[_currFileId];
            _uploadStream = new UploadStream(currFile.FullPath, _cloud, currFile.Size);
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
            long diff = _bytesWrote + count - _files[_currFileId].Size;

            if (diff > 0)
            {
                var zbuffer = new byte[count - diff];
                Array.Copy(buffer, 0, zbuffer, 0, count - diff);
                long zcount = count;

                _uploadStream.Write(zbuffer, offset, (int)(zcount - diff));

                NextFile();
            }

            long ncount = diff <= 0 ? count : diff;
            var nbuffer = new byte[ncount];
            Array.Copy(buffer, count - ncount, nbuffer, 0, ncount);

            _bytesWrote += ncount;

            _uploadStream.Write(nbuffer, offset, (int)ncount);
        }

        public event FileUploadedDelegate FileUploaded;

        private void OnFileUploaded(IEnumerable<File> files)
        {
            var e = FileUploaded;
            e?.Invoke(files);
        }

        public override void Close()
        {
            if (_files.Count > 1)
            {
                string content = $"filename={_origfile.Name}\r\nsize = {_origfile.Size.DefaultValue}"; //TODO: calculate CRC32 \r\ncrc32 = C9EB1402\r\n";
                var data = Encoding.UTF8.GetBytes(content);
                var stream = new UploadStream(_origfile.FullPath, _cloud, data.Length);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            ////remove test file created with webdav
            //var dele = new RemoveRequest(_cloud, _origfile.FullPath)
            //    .MakeRequestAsync().Result;

            _uploadStream?.Close();

            OnFileUploaded(_files);
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
