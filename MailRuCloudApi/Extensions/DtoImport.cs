using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MailRuCloudApi.Api;
using MailRuCloudApi.Api.Requests.Types;
using MailRuCloudApi.EntryTypes;
using MailRuCloudApi.PathResolve;
using File = MailRuCloudApi.EntryTypes.File;

namespace MailRuCloudApi.Extensions
{
    public static class DtoImport
    {
        public static DiskUsage ToDiskUsage(this AccountInfoResult data)
        {
            var res = new DiskUsage
            {
                Total = data.body.cloud.space.total * 1024 * 1024,
                Used = data.body.cloud.space.used * 1024 * 1024,
                OverQuota = data.body.cloud.space.overquota
            };
            return res;
        }




        public static ShardInfo ToShardInfo(this ShardInfoResult webdata, ShardType shardType)
        {
            List<ShardSection> shard;

            switch (shardType)
            {
                case ShardType.Video:
                    shard = webdata.body.video;
                    break;
                case ShardType.ViewDirect:
                    shard = webdata.body.view_direct;
                    break;
                case ShardType.WeblinkView:
                    shard = webdata.body.weblink_view;
                    break;
                case ShardType.WeblinkVideo:
                    shard = webdata.body.weblink_video;
                    break;
                case ShardType.WeblinkGet:
                    shard = webdata.body.weblink_get;
                    break;
                case ShardType.WeblinkThumbnails:
                    shard = webdata.body.weblink_thumbnails;
                    break;
                case ShardType.Auth:
                    shard = webdata.body.auth;
                    break;
                case ShardType.View:
                    shard = webdata.body.view;
                    break;
                case ShardType.Get:
                    shard = webdata.body.get;
                    break;
                case ShardType.Upload:
                    shard = webdata.body.upload;
                    break;
                case ShardType.Thumbnails:
                    shard = webdata.body.thumbnails;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shardType), shardType, null);
            }

            if (null == shard || shard.Count == 0)
                throw new Exception("Cannot get shard info");

            var res = new ShardInfo
            {
                Type = shardType,
                Count = int.Parse(shard[0].count),
                Url = shard[0].url

            };

            return res;
        }

        private static readonly string[] FolderKinds = { "folder", "camera-upload", "mounted", "shared" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="linkedToPath">parent path where shared item linked is. Empty if it's not a shared item.</param>
        /// <param name="shareUrl">shared item url. Empty if it's not a shared item.</param>
        /// <returns></returns>
        public static IFileOrFolder ToEntry(this FolderInfoResult data, string linkedToPath, string shareUrl)
        {

            if (!string.IsNullOrEmpty(shareUrl))
            {
                bool isFile = data.body.list.Any(it => it.weblink.TrimStart('/') == shareUrl.TrimStart('/'));

                string trimpath = linkedToPath;
                if (isFile) trimpath = WebDavPath.Parent(linkedToPath);

                foreach (var propse in data.body.list)
                {
                    propse.home = WebDavPath.Combine(trimpath, propse.name);
                }
                data.body.home = trimpath;
            }

            // when using ItemInfoRequest instead of FolderInfoRequest here's may be file type
            if (data.body.kind == "file") return data.ToFile();

            // mailru returns parent folder if asked for file path
            if (data.body.home == linkedToPath)
            {
                var folder = new Folder(linkedToPath)
                {
                    Files = data.body.list?
                        .Where(it => it.kind == "file")
                        .Select(it => it.ToFile()).ToList(),
                    Folders = data.body.list ?
                            .Where(it => FolderKinds.Contains(it.kind))
                            .Select(it => it.ToFolder())
                            .ToList(),
                    CreationTimeUtc = DateTime.Now,
                    LastAccessTimeUtc = DateTime.Now,
                    LastWriteTimeUtc = DateTime.Now,
                    Attributes = FileAttributes.Directory
                };
                return folder;
            }

            //file
            var fa = data.body.list.FirstOrDefault(k => k.home == linkedToPath);

            return fa?.ToFile();

            //TODO: don't remember what is this shit about
            //string parentPath = WebDavPath.Parent(path);
            //item = Cloud.Instance(httpContext).GetItems(parentPath).Result;
            //if (item != null)
            //{
            //    var f = item.Files.FirstOrDefault(k => k.FullPath == path);
            //    return null != f
            //        ? Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, f, IsWritable))
            //        : null;
            //}
        }


        private static File ToFile(this FolderInfoResult prop)
        {
            var file = new File(prop.body.home ?? "/" + prop.body.name, prop.body.size, string.Empty)
            {
                PublicLink = string.IsNullOrEmpty(string.Empty) ? "" : ConstSettings.PublishFileLink + string.Empty,
                PrimaryName = prop.body.name,
                CreationTimeUtc =  DateTime.Now, //because mail.ru doesn't send file time in this case
                LastAccessTimeUtc = DateTime.Now,
                LastWriteTimeUtc = DateTime.Now,
            };

            return file;
        }

        

        private static File ToFile(this FolderInfoProps prop)
        {
            var file = new File(prop.home, prop.size, prop.hash)
            {
                PublicLink = string.IsNullOrEmpty(prop.weblink) ? "" : ConstSettings.PublishFileLink + prop.weblink,
                PrimaryName = prop.name,
                CreationTimeUtc = UnixTimeStampToDateTime(prop.mtime),
                LastAccessTimeUtc = UnixTimeStampToDateTime(prop.mtime),
                LastWriteTimeUtc = UnixTimeStampToDateTime(prop.mtime),
            };

            return file;
        }

        private static Folder ToFolder(this FolderInfoProps prop)
        {
            var folder = new Folder(prop.size,
                prop.home,
                string.IsNullOrEmpty(prop.weblink) ? "" : ConstSettings.PublishFileLink + prop.weblink);
            return folder;
        }

        

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }



        public static IFileOrFolder ToEntry(this ItemLink itemlink, string linkpath)
        {
            if (!itemlink.IsFile)
                return new Folder(0, linkpath, itemlink.Href + itemlink.Name)
                {
                    CreationTimeUtc = itemlink.CreationDate ?? DateTime.Now.AddDays(-1),
                };

            return new File(linkpath, itemlink.Size, string.Empty)
                {
                    CreationTimeUtc = itemlink.CreationDate ?? DateTime.Now.AddDays(-1),
                };
        }

    }
}
