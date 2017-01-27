namespace MailRuCloudApi.Api.Streams
{
    //class EncodeUploadStream : SplittedUploadStream
    //{
    //    public EncodeUploadStream(string destinationPath, CloudApi cloud, string encPassword, long size, IFileSplitter fileSplitter) : base(destinationPath, cloud, size, fileSplitter)
    //    {
    //        var keys = new KeyGenerator(encPassword);
    //        _xts = XtsAes256.Create(keys.SecretKey, keys.InitVector);
    //        //_bytesWrote = _keys.InitVector.Length;
    //        base.Write(keys.InitVector, 0, keys.InitVector.Length);

    //    }

    //    private readonly Xts _xts;



    //    //private readonly byte[] _initVector = new byte[32];
    //    ////{
    //    ////        0x21, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
    //    ////        0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22
    //    ////};

    //    //private readonly byte[] _secretKey; //; = new byte[32];
    //    ////{
    //    ////        0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
    //    ////        0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11
    //    ////};


    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        var plain = buffer;
    //        var encrypted = new byte[buffer.Length];
    //        //var decrypted = new byte[buffer.Length];


    //        using (var transform = _xts.CreateEncryptor())
    //        {
    //            var bytes = transform.TransformBlock(plain, 0, count, encrypted, 0, 0x123456789a);
    //        }

    //        //using (var transform = xts.CreateDecryptor())
    //        //{
    //        //    var bytes = transform.TransformBlock(encrypted, 0, count, decrypted, 0, 0x123456789a);
    //        //}


    //        //if (!plain.SequenceEqual(decrypted,))
    //        //{
    //        //    throw new Exception("encode error");
    //        //}

    //        base.Write(encrypted, offset, count);
    //    }
    //}
}
