﻿using System;
using System.Collections.Generic;
using System.Linq;
using MailRuCloudApi.Api;
using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Extensions
{
    public static class DtoImport
    {
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

        public static Entry ToEntry(this FolderInfoResult data)
        {
            var entry = new Entry(
                    data.body.list
                        .Where(it => it.kind == "folder" || it.kind == "camera-upload")
                        .Select(it => new Folder(it.count.folders, it.count.files, it.size, it.home, string.IsNullOrEmpty(it.weblink) ? "" : ConstSettings.PublishFileLink + it.weblink))
                        .ToList(),
                    data.body.list
                        .Where(it => it.kind == "file")
                        .Select(it => new File(it.home, it.size, it.hash)
                        {
                            PublicLink =
                                string.IsNullOrEmpty(it.weblink) ? "" : ConstSettings.PublishFileLink + it.weblink,
                            PrimaryName = it.name,
                            CreationTimeUtc = UnixTimeStampToDateTime(it.mtime),
                            LastAccessTimeUtc = UnixTimeStampToDateTime(it.mtime),
                            LastWriteTimeUtc = UnixTimeStampToDateTime(it.mtime),
                        }).ToList(),
                    data.body.home)
                {Size = data.body.size};

            return entry;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
