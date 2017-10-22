using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MailRuCloudApi.Api;
using MailRuCloudApi.Api.Requests;
using MailRuCloudApi.EntryTypes;
using MailRuCloudApi.Extensions;
using Newtonsoft.Json;
using NWebDav.Server.Logging;

namespace MailRuCloudApi.PathResolve
{
    public class PathResolver
    {
        private static readonly ILogger Logger = LoggerFactory.Factory.CreateLogger(typeof(PathResolver));

        public static string LinkContainerName = "item.links.wdmrc";
        private readonly CloudApi _api;
        private ItemList _itemList;


        public PathResolver(CloudApi api)
        {
            _api = api;

            Load();
        }

        

        public void Save()
        {
            Logger.Log(LogLevel.Info, () => $"Saving links to {LinkContainerName}");

            string content = JsonConvert.SerializeObject(_itemList, Formatting.Indented);
            var data = Encoding.UTF8.GetBytes(content);
            using (var stream = new UploadStream("/" + LinkContainerName, _api, data.Length))
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
        }

        public void Load()
        {
            Logger.Log(LogLevel.Info, () => $"Loading links from {LinkContainerName}");
            var flist = new FolderInfoRequest(_api, WebDavPath.Root).MakeRequestAsync().Result.ToEntry(new Resolved(){Path = WebDavPath.Root});
            var file = (flist as Folder)?.Files.FirstOrDefault(f => f.Name == LinkContainerName);
            if (file != null && file.Size > 3) //some clients put one/two/three-byte file before original file
            {
                DownloadStream stream = new DownloadStream(file, _api);

                using (StreamReader reader = new StreamReader(stream))
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    var ser = new JsonSerializer();
                    _itemList = ser.Deserialize<ItemList>(jsonReader);
                }
            }

            if (null == _itemList) _itemList = new ItemList();

            foreach (var f in _itemList.Items)
            {
                f.MapTo = WebDavPath.Clean(f.MapTo);
            }
        }

        public List<ItemLink> GetItems(string path)
        {
            var z = _itemList.Items
                .Where(f => f.MapTo == path)
                .ToList();

            return z;
        }

        public ItemLink GetItem(string path)
        {
            var name = WebDavPath.Name(path);
            var pa = WebDavPath.Parent(path);

            var item = _itemList.Items
                .FirstOrDefault(f => f.MapTo == pa && f.Name == name);

            return item;
        }

        public void RemoveItem(string path)
        {
            var name = WebDavPath.Name(path);
            var pa = WebDavPath.Parent(path);

            var z = _itemList.Items
                .FirstOrDefault(f => f.MapTo == pa && f.Name == name);

            if (z != null)
            {
                _itemList.Items.Remove(z);
                Save();
            }


        }


        public class Resolved
        {
            public string RelationalUrl { get; set; } = string.Empty;
            public string BasePath { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
        }

        public Resolved AsWebLink(string path)
        {
            //TODO: subject to refact
            string parent = path;
            ItemLink wp;
            string right = string.Empty;
            do
            {
                string name = WebDavPath.Name(parent);
                parent = WebDavPath.Parent(parent);
                wp = _itemList.Items.FirstOrDefault(ip => parent == ip.MapTo && name == ip.Name);
                if (wp != null) right = WebDavPath.Combine(name, right);
            } while (parent != WebDavPath.Root  && wp == null);

            return new Resolved()
            {
                RelationalUrl = wp == null
                    ? string.Empty
                    : wp.Href + (wp.IsFile ? string.Empty : right),
                BasePath = wp?.MapTo ?? string.Empty,
                Path = path
            };
        }



        public Resolved AsRelationalWebLink(string path)
        {
            //TODO: subject to refact
            var data = AsWebLink(path);
            data.RelationalUrl = GetRelaLink(data.RelationalUrl);
            return data;
        }

        public void Add(string url, string path, string name, bool isFile, long size, DateTime? creationDate)
        {
            Load();

            path = WebDavPath.Clean(path);
            url = GetRelaLink(url);

            if (!_itemList.Items.Any(ii => ii.MapTo == path && ii.Name == name))
            {
                _itemList.Items.Add(new ItemLink
                {
                    Href = url,
                    MapTo = WebDavPath.Clean(path),
                    Name = name,
                    IsFile = isFile,
                    Size = size,
                    CreationDate = creationDate
                });
                Save();
            }
        }




        private const string PublicBaseLink = "https://cloud.mail.ru/public";
        private const string PublicBaseLink1 = "https:/cloud.mail.ru/public";

        private string GetRelaLink(string url)
        {
            if (url.StartsWith(PublicBaseLink)) return url.Remove(PublicBaseLink.Length);
            if (url.StartsWith(PublicBaseLink1)) return url.Remove(PublicBaseLink1.Length);
            return url;
        }


    }
}