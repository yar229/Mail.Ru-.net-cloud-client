//-----------------------------------------------------------------------
// <created file="Entry.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    using System.Collections.Generic;

    /// <summary>
    /// List of items in cloud.
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entry" /> class.
        /// </summary>
        /// <param name="folders">List of the folders.</param>
        /// <param name="files">List of the files.</param>
        /// <param name="path">The entry path on the server.</param>
        public Entry(IList<Folder> folders, IList<File> files, string path)
        {
            NumberOfFolders = folders.Count;
            NumberOfFiles = files.Count;
            Folders = folders;
            Files = files;
            FullPath = path;
        }

        /// <summary>
        /// Gets number of the folders.
        /// </summary>
        public int NumberOfFolders { get; internal set; }

        /// <summary>
        /// Gets number of the files.
        /// </summary>
        public int NumberOfFiles { get; internal set; }

        public int NumberOfItems => NumberOfFolders + NumberOfFiles;

        /// <summary>
        /// Gets list of the folders with their specification.
        /// </summary>
        public IList<Folder> Folders { get; internal set; }

        /// <summary>
        /// Gets list of the files with their specification.
        /// </summary>
        public IList<File> Files { get; internal set; }

        /// <summary>
        /// Gets full entry path on the server.
        /// </summary>
        public string FullPath { get; internal set; }
    }
}
