using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MailRuCloudApi.Api;
using MailRuCloudApi.Api.Requests;
using MailRuCloudApi.Extensions;
using Newtonsoft.Json;

namespace MailRuCloudApi.PathResolve
{
    public class PathResolver
    {
        private const string LinkContainerName = "folder.links.wdmrc";
        private readonly CloudApi _api;
        private FolderList _folderList;


        public PathResolver(CloudApi api)
        {
            _api = api;

            Load();
        }

        

        public void Save()
        {
            string content = JsonConvert.SerializeObject(_folderList);
            var data = Encoding.UTF8.GetBytes(content);
            var stream = new UploadStream("/" + LinkContainerName, _api, data.Length);
            stream.Write(data, 0, data.Length);
            stream.Close();
        }

        public void Load()
        {
            var flist = new FolderInfoRequest(_api, "/").MakeRequestAsync().Result.ToEntry();
            var file = flist.Files.FirstOrDefault(f => f.Name == LinkContainerName);
            if (file != null)
            {
                DownloadStream stream = new DownloadStream(new List<File>() {file}, _api, null, null);

                using (StreamReader reader = new StreamReader(stream))
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    var ser = new JsonSerializer();
                    _folderList = ser.Deserialize<FolderList>(jsonReader);
                }
            }

            if (null == _folderList) _folderList = new FolderList();

            foreach (var f in _folderList.Folders)
            {
                f.MapTo = WebDavPath.Clean(f.MapTo);
            }
        }

        public List<FolderLink> GetItems(string path)
        {
            var z = _folderList.Folders
                .Where(f => f.MapTo == path)
                .ToList();

            return z;
        }

        

        public string AsWebLink(string path)
        {
            //TODO: subject to refact
            string parent = path;
            string wp;
            string right = string.Empty;
            do
            {
                string name = WebDavPath.Name(parent);
                parent = WebDavPath.Parent(parent);
                wp = _folderList.Folders.FirstOrDefault(ip => parent == ip.MapTo && name == ip.Name)?.Href;
                if (string.IsNullOrEmpty(wp)) right = WebDavPath.Combine(name, right);
            } while (parent != "/" && string.IsNullOrEmpty(wp));
            
            
            return string.IsNullOrEmpty(wp)
                ? string.Empty
                : wp + right;
        }


        private const string PublicBaseLink = "https://cloud.mail.ru/public";
        private const string PublicBaseLink1 = "https:/cloud.mail.ru/public";
        public string AsRelationalWebLink(string path)
        {
            //TODO: subject to refact
            string link = AsWebLink(path);
            if (!string.IsNullOrEmpty(link))
            {
                if (link.StartsWith(PublicBaseLink))
                {
                    return link.Substring(PublicBaseLink.Length - 1);
                }
                return link;
            }
            return String.Empty;

        }

        public void Add(string url, string path, string name)
        {
            Load();

            if (url.StartsWith(PublicBaseLink)) url = url.Remove(PublicBaseLink.Length);
            if (url.StartsWith(PublicBaseLink1)) url = url.Remove(PublicBaseLink1.Length);
            _folderList.Folders.Add(new FolderLink
            {
                Href = url,
                MapTo = WebDavPath.Clean(path),
                Name = name
            });
            Save();
        }
    }
}