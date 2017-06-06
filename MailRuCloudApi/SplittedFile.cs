using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MailRuCloudApi
{
    public class SplittedFile : File
    {
        public SplittedFile(IList<File> files)
        {
            FileHeader = files.First(f => !Regex.Match(f.Name, @".wdmrc.\d\d\d\Z").Success);
            _fileParts.AddRange(files
                .Where(f => Regex.Match(f.Name, @".wdmrc.\d\d\d\Z").Success)
                .OrderBy(f => f.Name));

            FullPath = FileHeader.FullPath;
        }


        public override FileSize Size => _fileParts.Sum(f => f.Size.DefaultValue);

        public override string Hash => FileHeader.Hash;

        public override DateTime CreationTimeUtc => FileHeader.CreationTimeUtc;
        public override DateTime LastWriteTimeUtc => FileHeader.LastWriteTimeUtc;
        public override DateTime LastAccessTimeUtc => FileHeader.LastAccessTimeUtc;

        public override List<File> Files => _fileParts;

        private File FileHeader { get; }
        private readonly List<File> _fileParts = new List<File>();
    }
}