namespace MailRuCloudApi.Api.Streams.FileInfo
{
    class FileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public int Crc32 { get; set; }
        public byte[] Key { get; set; }

    }
}
