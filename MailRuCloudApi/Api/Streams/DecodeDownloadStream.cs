using System;
using System.Collections.Generic;
using MailRuCloudApi.Api.Crypt;
using MailRuCloudApi.Api.Streams.Transformers;
using XTSSharp;

namespace MailRuCloudApi.Api.Streams
{
    //class DecodeDownloadStream : DownloadStream
    //{
    //    public DecodeDownloadStream(File file, CloudApi cloud, string password, long? start, long? end, Func<byte[], IByteTransformer> transformerFunc): base(file, cloud, start, end)
    //    {
    //        var headerStream = new DownloadStream(file, cloud, 0, 32);


    //    }



    //    public override int Read(byte[] buffer, int offset, int count)
    //    {

    //        var readed =  base.Read(buffer, offset, count);

            

    //        if (readed > 0)
    //        {
    //            var xts = XtsAes256.Create(_keys.SecretKey, _keys.InitVector);

    //            var decrypted = new byte[buffer.Length];

    //            long bytes;
    //            using (var transform = xts.CreateDecryptor())
    //            {
    //                bytes  = transform.TransformBlock(buffer, 0, readed, decrypted, 0, 0x123456789a);
    //            }

    //            //var plain = new byte[bytes];
    //            //Array.Copy(decrypted, plain, bytes);
    //            //var str = System.Text.Encoding.UTF8.GetString(plain);

    //            Array.Copy(decrypted, buffer, readed);
    //        }

    //        return readed;
    //    }
    //}
}
