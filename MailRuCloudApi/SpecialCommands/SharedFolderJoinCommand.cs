using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Stores;

namespace MailRuCloudApi.SpecialCommands
{
    public class SharedFolderJoinCommand: SpecialCommand
    {
        private readonly MailRuCloud _cloud;
        private readonly string _path;
        private readonly string _param;

        public SharedFolderJoinCommand(MailRuCloud cloud, string path, string param)
        {
            _cloud = cloud;
            _path = path;
            _param = param;
        }

        private string Value
        {
            get
            {
                var m = Regex.Match(_param, @"(?snx-) (https://?cloud.mail.ru/public)?(?<data>/\w*/?\w*)/?\s*");

                return m.Success
                    ? m.Groups["data"].Value
                    : string.Empty;
            }
        }

        public override Task<StoreCollectionResult> Execute()
        {
            bool k = _cloud.CloneItem(_path, Value).Result;
            return Task.FromResult(new StoreCollectionResult(k ? DavStatusCode.Created : DavStatusCode.PreconditionFailed));
        }
    }
}