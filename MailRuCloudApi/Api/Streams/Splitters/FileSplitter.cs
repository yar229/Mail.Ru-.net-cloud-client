using System;
using System.Collections.Generic;

namespace MailRuCloudApi.Api.Streams.Splitters
{
    class FileSplitter : IFileSplitter
    {
        private readonly long _maxSize;
        private readonly bool _unconditionalSplit;

        public FileSplitter(long maxSize, bool unconditionalSplit =false)
        {
            _maxSize = maxSize;
            _unconditionalSplit = unconditionalSplit;
        }
        public List<File> SplitFile(File origfile)
        {
            var files = new List<File>();

            long allowedSize = _maxSize; //TODO: make it right //- BytesCount(_file.Name);
            if (origfile.Size.DefaultValue <= allowedSize && !_unconditionalSplit)
            {
                files.Add(origfile);
            }
            else
            {
                int nfiles = (int)(origfile.Size.DefaultValue / allowedSize + 1);
                if (nfiles > 999)
                    throw new OverflowException("Cannot upload more than 999 file parts");
                for (int i = 1; i <= nfiles; i++)
                {
                    var f = new File($"{origfile.FullPath}.wdmrc.{i:D3}",
                        i != nfiles ? allowedSize : origfile.Size.DefaultValue % allowedSize,
                        null);
                    files.Add(f);
                }
            }
            return files;
        }
    }
}
