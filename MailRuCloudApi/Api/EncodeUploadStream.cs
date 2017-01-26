using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Crypt;
using XTSSharp;

namespace MailRuCloudApi.Api
{
    class EncodeUploadStream : SplittedUploadStream
    {
        public EncodeUploadStream(string destinationPath, CloudApi cloud, string encPassword, long size) : base(destinationPath, cloud, size)
        {
            _keys = new KeyGenerator(encPassword);

            //_bytesWrote = _keys.InitVector.Length;
            //base.Write(_keys.InitVector, 0, _keys.InitVector.Length);
        }

        private readonly KeyGenerator _keys;


        //private readonly byte[] _initVector = new byte[32];
        ////{
        ////        0x21, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
        ////        0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22
        ////};

        //private readonly byte[] _secretKey; //; = new byte[32];
        ////{
        ////        0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
        ////        0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11
        ////};


        public override void Write(byte[] buffer, int offset, int count)
        {
            var xts = XtsAes256.Create(_keys.SecretKey, _keys.InitVector);
            

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
