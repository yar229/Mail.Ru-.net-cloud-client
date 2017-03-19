using System;
using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Api.Requests
{
    class FolderInfoRequest : BaseRequest<FolderInfoResult>
    {
        private readonly string _path;
        private readonly int _offset;
        private readonly int _limit;

        public FolderInfoRequest(CloudApi cloudApi, string path, int offset = 0, int limit = int.MaxValue) : base(cloudApi)
        {
            _path = path;
            _offset = offset;
            _limit = limit;
        }

        public override string RelationalUri
        {
            get
            {
                var uri = $"/api/v2/folder?token={CloudApi.Account.AuthToken}&home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";
                return uri;
            }
        }
    }
}
