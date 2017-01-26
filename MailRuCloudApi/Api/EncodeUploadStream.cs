using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XTSSharp;

namespace MailRuCloudApi.Api
{
    class EncodeUploadStream : SplittedUploadStream
    {
        public EncodeUploadStream(string destinationPath, CloudApi cloud, string encPassword, long size) : base(destinationPath, cloud, size)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(_initVector);
            }

            byte[] salt = CreateRandomSalt(7);

            PasswordDeriveBytes cdk = new PasswordDeriveBytes(encPassword, salt);
            _secretKey = cdk.CryptDeriveKey("RC2", "SHA1", 256, _initVector);
        }

        public static byte[] CreateRandomSalt(int length)
        {
            var randBytes = length >= 1 ? new byte[length] : new byte[1];

            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            rand.GetBytes(randBytes);
            return randBytes;
        }

        private readonly byte[] _initVector = new byte[32];
        //{
        //        0x21, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
        //        0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22
        //};

        private readonly byte[] _secretKey; //; = new byte[32];
        //{
        //        0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
        //        0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11
        //};


        public override void Write(byte[] buffer, int offset, int count)
        {
            var xts = XtsAes256.Create(_secretKey, _initVector);
            

            var plain = buffer;
            var encrypted = new byte[buffer.Length];
            //var decrypted = new byte[buffer.Length];


            using (var transform = xts.CreateEncryptor())
            {
                var bytes = transform.TransformBlock(plain, 0, count, encrypted, 0, 0x123456789a);
            }

            //using (var transform = xts.CreateDecryptor())
            //{
            //    var bytes = transform.TransformBlock(encrypted, 0, count, decrypted, 0, 0x123456789a);
            //}


            //if (!plain.SequenceEqual(decrypted,))
            //{
            //    throw new Exception("encode error");
            //}

            base.Write(encrypted, offset, count);
        }
    }
}
