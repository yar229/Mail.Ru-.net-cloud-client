using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Stores;

namespace MailRuCloudApi.SpecialCommands
{
    public class SharedFolderLinkCommand : SpecialCommand
    {
        private readonly MailRuCloud _cloud;
        private readonly string _path;
        private readonly string _param;

        public SharedFolderLinkCommand(MailRuCloud cloud, string path, string param)
        {
            _cloud = cloud;
            _path = path;
            _param = param;
        }

        private string Value
        {
            get
            {
                var m = Regex.Match(_param, @"(?snx-) /? >> (https://?cloud.mail.ru/public)?(?<data>/\w*/\w*)/?\s*");

                return m.Success
                    ? m.Groups["data"].Value
                    : string.Empty;
            }
        }


        public override Task<StoreCollectionResult> Execute()
        {
            var m = Regex.Match(_param, @"(?snx-)link \s+ (https://?cloud.mail.ru/public)?(?<url>/\w*/\w*)/? \s* (?<name>.*) ");
            if (m.Success)
            {
                _cloud.LinkFolder(m.Groups["url"].Value, _path, m.Groups["name"].Value);
            }

            return Task.FromResult(new StoreCollectionResult(DavStatusCode.Created));
        }
    }
}