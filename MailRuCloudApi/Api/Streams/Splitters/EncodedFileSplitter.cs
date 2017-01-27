using System.Collections.Generic;

namespace MailRuCloudApi.Api.Streams.Splitters
{
    class EncodedFileSplitter : IFileSplitter
    {
        private readonly FileSplitter _splitter;


        public EncodedFileSplitter(long maxSize)
        {
            _splitter = new FileSplitter(maxSize, true);
        }

        public List<File> SplitFile(File origfile)
        {
            return _splitter.SplitFile(origfile);
        }
    }
}