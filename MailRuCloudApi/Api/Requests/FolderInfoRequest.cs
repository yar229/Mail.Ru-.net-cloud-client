using System;
using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Api.Requests
{
    class FolderInfoRequest : BaseRequest<FolderInfoResult>
    {
        private readonly string _path;
        private readonly bool _isWebLink;
        private readonly int _offset;
        private readonly int _limit;

        public FolderInfoRequest(CloudApi cloudApi, string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue) : base(cloudApi)
        {
            _path = path;
            _isWebLink = isWebLink;
            _offset = offset;
            _limit = limit;
        }

        public override string RelationalUri
        {
            get
            {
                var uri = _isWebLink
                    ? $"/api/v2/folder?token={CloudApi.Account.AuthToken}&weblink={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}"
                    : $"/api/v2/folder?token={CloudApi.Account.AuthToken}&home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";
                return uri;
            }
        }
    }

    class DownloadTokenRequest : BaseRequest<DownloadTokenResult>
    {
        public DownloadTokenRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        public override string RelationalUri
        {
            get
            {
                var uri = $"/api/v2/tokens/download?token={CloudApi.Account.AuthToken}";
                return uri;
            }
        }
    }

    public class DownloadTokenBody
    {
        public string token { get; set; }
    }

    public class DownloadTokenResult
    {
        public string email { get; set; }
        public DownloadTokenBody body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }
}
