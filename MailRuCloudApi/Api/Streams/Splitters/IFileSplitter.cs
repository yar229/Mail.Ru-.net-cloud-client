using System.Collections.Generic;

namespace MailRuCloudApi.Api.Streams.Splitters
{
    interface IFileSplitter
    {
        List<File> SplitFile(File origfile);
    }
}