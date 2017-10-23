﻿//-----------------------------------------------------------------------
// <created file="File.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MailRuCloudApi
{
    /// <summary>
    /// Server file info.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullPath) + "}")]
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
        //TODO: refact
        public virtual string Name => FullPath.Substring(FullPath.LastIndexOf("/", StringComparison.Ordinal) + 1);

        public string Extension => System.IO.Path.GetExtension(Name);

        /// <summary>
        /// Gets file hash value.
        /// </summary>
        /// <value>File hash.</value>
        public virtual string Hash
        {
            get => _hash;
            internal set => _hash = value;
        }

        /// <summary>
        /// Gets file size.
        /// </summary>
        /// <value>File size.</value>
        public virtual FileSize Size
        {
            get => _size;
            set => _size = value;
        }

        /// <summary>
        /// Gets full file path with name in server.
        /// </summary>
        /// <value>Full file path.</value>
        public string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value.Replace("\\", "/");
                if (!string.IsNullOrEmpty(Name) && !_fullPath.EndsWith("/" + Name)) _fullPath = _fullPath.TrimEnd('/') + "/" + Name;
            }
        }

        public string Path => WebDavPath.Parent(FullPath);

        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; internal set; }

        public virtual List<File> Files => new List<File> {this};

        /// <summary>
        /// Gets or sets base file size.
        /// </summary>
        /// <value>File size.</value>
        internal FileSize PrimarySize => Size;

        public virtual DateTime CreationTimeUtc { get; set; }
        public virtual DateTime LastWriteTimeUtc { get; set; }
        public virtual DateTime LastAccessTimeUtc { get; set; }
        public bool IsSplitted => Files.Any(f => f.FullPath != FullPath);

        public void SetName(string destinationName)
        {
            string path = WebDavPath.Parent(FullPath);
            FullPath = WebDavPath.Combine(path, destinationName);
            if (Files.Count > 1)
                foreach (var fiFile in Files)
                {
                    fiFile.FullPath = WebDavPath.Combine(path, destinationName + ".wdmrc" + fiFile.Extension); //TODO: refact
                }
        }

        public void SetPath(string fullPath)
        {
            FullPath = WebDavPath.Combine(fullPath, Name);
            if (Files.Count > 1)
                foreach (var fiFile in Files)
                {
                    fiFile.FullPath = WebDavPath.Combine(fullPath, fiFile.Name); //TODO: refact
                }
        }
    }
}

