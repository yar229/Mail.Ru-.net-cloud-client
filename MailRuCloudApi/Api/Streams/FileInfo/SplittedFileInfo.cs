using System.Collections.Generic;

namespace MailRuCloudApi.Api.Streams.FileInfo
{
    class SplittedFileInfo : FileInfo
    {
        public List<FileInfo> Parts { get; set; } = new List<FileInfo>();
    }
}