using System;
using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Api.Requests
{
    class FolderInfoRequest : BaseRequest<FolderInfoResult>
    {
        private readonly string _path;

        public FolderInfoRequest(CloudApi cloudApi, string path) : base(cloudApi)
        {
            _path = path;
        }

        public override string RelationalUri
        {
            get
            {
                var uri = $"/api/v2/folder?token={CloudApi.Account.AuthToken}&home={Uri.EscapeDataString(_path)}";
                return uri;
            }
        }
    }
}
