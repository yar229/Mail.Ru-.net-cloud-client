//-----------------------------------------------------------------------
// <created file="Folder.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MailRuCloudApi.EntryTypes
{
    /// <summary>
    /// Server file info.
    /// </summary>
    public class Folder : IFileOrFolder
    {
        private IList<File> _files = new List<File>();
        private IList<Folder> _folders = new List<Folder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder" /> class.
        /// </summary>
        public Folder(string fullPath)
        {
            FullPath = fullPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder" /> class.
        /// </summary>
        /// <param name="size">Folder size.</param>
        /// <param name="fullPath">Full folder path.</param>
        /// <param name="publicLink">Public folder link.</param>
        public Folder(FileSize size, string fullPath, string publicLink = null):this(fullPath)
        {
            Folders = new List<Folder>();
            Files = new List<File>();

            Size = size;
            PublicLink = publicLink;
        }

        /// <summary>
        /// Gets number of folders in folder.
        /// </summary>
        /// <value>Number of folders.</value>
        public int NumberOfFolders => Folders?.Count ?? 0;

        /// <summary>
        /// Gets number of files in folder.
        /// </summary>
        /// <value>Number of files.</value>
        public int NumberOfFiles => Files?.Count ?? 0;

        /// <summary>
        /// Gets folder name.
        /// </summary>
        /// <value>Folder name.</value>
        public string Name
        {
            get
            {
                string z = FullPath == "/" ? "" : FullPath.TrimEnd('/').Remove(0, FullPath.LastIndexOf('/') + 1);
                return z;
            }
        }

        /// <summary>
        /// Gets folder size.
        /// </summary>
        /// <value>Folder size.</value>
        public FileSize Size { get; }

        /// <summary>
        /// Gets full folder path on the server.
        /// </summary>
        /// <value>Full folder path.</value>
        public string FullPath
        {
            get;
        }

        /// <summary>
        /// Gets public folder link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; }

        public DateTime CreationTimeUtc { get; set; } = DateTime.Now.AddDays(-1);

        public DateTime LastWriteTimeUtc { get; set; } = DateTime.Now.AddDays(-1);


        public DateTime LastAccessTimeUtc { get; set; } = DateTime.Now.AddDays(-1);


        public FileAttributes Attributes { get; set; } = FileAttributes.Directory;


        /// <summary>
        /// Gets list of the folders with their specification.
        /// </summary>
        public IList<Folder> Folders
        {
            get { return _folders; }
            set { _folders = value; }
        }

        /// <summary>
        /// Gets list of the files with their specification.
        /// </summary>
        public IList<File> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        public void AddChild(IFileOrFolder item)
        {
            if (item is Folder f)
            {
                if (Folders.All(inf => inf.FullPath != item.FullPath))
                    Folders.Add(f);
            }
            else
            {
                if (Files.All(inf => inf.FullPath != item.FullPath))
                    Files.Add(item as File);;
            }
        }
    }
}
