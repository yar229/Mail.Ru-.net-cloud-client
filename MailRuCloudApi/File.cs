//-----------------------------------------------------------------------
// <created file="File.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MailRuCloudApi
{
    /// <summary>
    /// Server file info.
    /// </summary>
    public class File
    {
        protected File()
        {
        }

        public File(string fullPath, long size, string hash)
        {
            FullPath = fullPath;
            _size = size;
            _hash = hash;
        }


        private string _fullPath;
        private FileSize _size;
        private string _hash;

        /// <summary>
        /// Gets file name.
        /// </summary>
        /// <value>File name.</value>
        public virtual string Name => FullPath.Substring(FullPath.LastIndexOf("/", StringComparison.Ordinal) + 1);

        public string Extension => System.IO.Path.GetExtension(Name);

        /// <summary>
        /// Gets file hash value.
        /// </summary>
        /// <value>File hash.</value>
        public virtual string Hash
        {
            get { return _hash; }
            internal set { _hash = value; }
        }

        /// <summary>
        /// Gets file size.
        /// </summary>
        /// <value>File size.</value>
        public virtual FileSize Size
        {
            get { return _size; }
            internal set { _size = value; }
        }

        /// <summary>
        /// Gets full file path with name in server.
        /// </summary>
        /// <value>Full file path.</value>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }
            set
            {
                _fullPath = value.Replace("\\", "/");
                if (!string.IsNullOrEmpty(Name) && !_fullPath.EndsWith("/" + Name)) _fullPath = _fullPath.TrimEnd('/') + "/" + Name;
            }
        }

        public string Path
        {
            get
            {
                int index = FullPath.LastIndexOf(Name, StringComparison.Ordinal);
                string s = index >= 0 ? FullPath.Substring(0, index) : FullPath;
                //if (s.Length > 1 && s.EndsWith("/")) s = s.Remove(s.Length - 1, 1);
                return s;
            }
        }

        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; internal set; }

        /// <summary>
        /// Gets last modified time of file in UTC format.
        /// </summary>
        public DateTime LastModifiedTimeUTC { get; internal set; }

        public virtual List<File> Files => new List<File> {this};

        /// <summary>
        /// Gets or sets base file name.
        /// </summary>
        internal string PrimaryName { get; set; }

        /// <summary>
        /// Gets or sets base file size.
        /// </summary>
        /// <value>File size.</value>
        internal FileSize PrimarySize => Size;

        public virtual DateTime CreationTimeUtc { get; set; }
        public virtual DateTime LastWriteTimeUtc { get; set; }
        public virtual DateTime LastAccessTimeUtc { get; set; }
    }
}
