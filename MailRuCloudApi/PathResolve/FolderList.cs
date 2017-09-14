using System.Collections.Generic;

namespace MailRuCloudApi.PathResolve
{
    public class FolderList
    {
        public List<FolderLink> Folders { get; set; } = new List<FolderLink>();
    }


    //TODO: subject to refact

    public class FolderLink
    {
        public string Href { get; set; }
        public string MapTo { get; set; }
        public string Name { get; set; }
    }
}