using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Crypt;
using XTSSharp;

namespace MailRuCloudApi.Api.Streams.Transformers
{
    interface IByteTransformer
    {
        byte[] Transform(byte[] buffer, int offset, int count);
        byte[] Data { get; }
    }

    class XtsAes256Transformer : IByteTransformer
    {
        private readonly Xts _xts;
        private readonly KeyGenerator _keys;

        public XtsAes256Transformer(string password)
        {
            _keys = new KeyGenerator(password);
            _xts = XtsAes256.Create(_keys.SecretKey, _keys.InitVector);
        }


        public byte[] Transform(byte[] buffer, int offset, int count)
        {
            var encrypted = new byte[buffer.Length];
            using (var transform = _xts.CreateEncryptor())
            {
                var bytes = transform.TransformBlock(buffer, offset, count, encrypted, offset, 0x123456789a);
            }
            return encrypted;
        }

        public byte[] Data => _keys.InitVector;
    }
}
