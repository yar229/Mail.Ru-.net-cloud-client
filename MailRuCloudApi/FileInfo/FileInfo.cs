using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailRuCloudApi.FileInfo
{
    class SplittedFileInfo : FileInfo
    {
        public List<FileInfo> Parts { get; set; } = new List<FileInfo>();
    }

    class FileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public int Crc32 { get; set; }
        public byte[] Key { get; set; }

    }
}
