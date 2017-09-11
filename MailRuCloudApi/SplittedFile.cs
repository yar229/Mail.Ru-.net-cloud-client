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
            FileParts.AddRange(files
                .Where(f => Regex.Match(f.Name, @".wdmrc.\d\d\d\Z").Success)
                .OrderBy(f => f.Name));

            FullPath = FileHeader.FullPath; //FullPath = WebDavPath.Combine(FileHeader.Path, FileHeader.Name.Substring(0, FileHeader.Name.Length - HeaderSuffix.Length));
        }


        public override FileSize Size => FileParts.Sum(f => f.Size.DefaultValue);

        public override string Hash => FileHeader.Hash;

        public override DateTime CreationTimeUtc => FileHeader.CreationTimeUtc;
        public override DateTime LastWriteTimeUtc => FileHeader.LastWriteTimeUtc;
        public override DateTime LastAccessTimeUtc => FileHeader.LastAccessTimeUtc;

        public override List<File> Files => FileParts;

        public File FileHeader { get; set; }
        public List<File> FileParts = new List<File>();
    }
}