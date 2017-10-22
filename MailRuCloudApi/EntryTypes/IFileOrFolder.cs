using System;

namespace MailRuCloudApi.EntryTypes
{
    public interface IFileOrFolder
    {
        string FullPath { get;  }
        FileSize Size { get; }
        string Name { get; }
        DateTime CreationTimeUtc { get;}
        bool IsLink { get; set; }
    }
}