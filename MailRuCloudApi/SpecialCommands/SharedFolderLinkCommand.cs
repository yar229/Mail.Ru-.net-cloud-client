using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Stores;

namespace MailRuCloudApi.SpecialCommands
{
    public class SharedFolderLinkCommand: SpecialCommand
    {
        private readonly MailRuCloud _cloud;
        private readonly string _param;

        public SharedFolderLinkCommand(MailRuCloud cloud, string param)
        {
            _cloud = cloud;
            _param = param;
        }

        private string Value
        {
            get
            {
                var m = Regex.Match(_param, @"(?snx-) \A /? >> (https://?cloud.mail.ru/public)?(?<data>/\w*/\w*)/?\s*");

                return m.Success
                    ? m.Groups["data"].Value
                    : string.Empty;
            }
        }

        private string Path
        {
            get
            {
                int pos = _param.LastIndexOf("/>>", StringComparison.Ordinal);
                return pos > 0
                    ? _param.Substring(0, pos)
                    : "/";
            }
        }

        public override Task<StoreCollectionResult> Execute()
        {
            bool k = _cloud.CloneItem(Path, Value).Result;
            return Task.FromResult(new StoreCollectionResult(k ? DavStatusCode.Created : DavStatusCode.PreconditionFailed));
        }
    }
}