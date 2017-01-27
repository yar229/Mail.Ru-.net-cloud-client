using System;
using System.Collections.Generic;
using MailRuCloudApi.Api.Crypt;
using XTSSharp;

namespace MailRuCloudApi.Api.Streams
{
    class DecodeDownloadStream : DownloadStream
    {
        public DecodeDownloadStream(IList<File> files, CloudApi cloud, string password, long? start, long? end): base(files, cloud, start, end)
        {
            var headerStream = new DownloadStream(files, cloud, 0, 32);


            var iv = new byte[32];
            var bytes = headerStream.Read(iv, 0, 32);
            _keys = new KeyGenerator(password, iv);

            //_keys = new KeyGenerator(password);
        }

        private readonly KeyGenerator _keys;


        private bool _first = true;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_first)
            {
                var bf = new byte[32];
                var readedf = base.Read(bf, 0, 32);
                _first = false;
            }

            var readed =  base.Read(buffer, offset, count);

            

            if (readed > 0)
            {

                //var secretKey = new byte[]
                //{
                //    0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
                //    0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11
                //};
                //var initVector = new byte[]
                //{
                //    0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
                //    0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22
                //};

                var xts = XtsAes256.Create(_keys.SecretKey, _keys.InitVector);

                var decrypted = new byte[buffer.Length];

                long bytes;
                using (var transform = xts.CreateDecryptor())
                {
                    bytes  = transform.TransformBlock(buffer, 0, readed, decrypted, 0, 0x123456789a);
                }

                //var plain = new byte[bytes];
                //Array.Copy(decrypted, plain, bytes);
                //var str = System.Text.Encoding.UTF8.GetString(plain);

                Array.Copy(decrypted, buffer, readed);
            }

            return readed;
        }
    }
}
