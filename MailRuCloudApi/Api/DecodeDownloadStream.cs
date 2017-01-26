using System;
using System.Collections.Generic;
using XTSSharp;

namespace MailRuCloudApi.Api
{
    class DecodeDownloadStream : DownloadStream
    {
        public DecodeDownloadStream(IList<File> files, CloudApi cloud, long? start, long? end) : base(files, cloud, start, end)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed =  base.Read(buffer, offset, count);

            if (readed > 0)
            {

                var secretKey = new byte[]
                {
                    0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11
                };
                var initVector = new byte[]
                {
                    0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
                    0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22
                };

                var xts = XtsAes256.Create(secretKey, initVector);

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
