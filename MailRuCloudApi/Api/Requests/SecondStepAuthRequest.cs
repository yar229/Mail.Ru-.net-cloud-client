using System;
using System.Text;

namespace MailRuCloudApi.Api.Requests
{
    class SecondStepAuthRequest : BaseRequest<string>
    {
        private readonly string _csrf;
        private readonly string _login;
        private readonly string _authCode;
        private readonly bool _doNotAskAgainForThisDevice;

        public SecondStepAuthRequest(CloudApi cloudApi, string csrf, string login, string authCode, bool doNotAskAgainForThisDevice) : base(cloudApi)
        {
            _csrf = csrf;
            _login = login;
            _authCode = authCode;
            _doNotAskAgainForThisDevice = doNotAskAgainForThisDevice;
        }

        public override string RelationalUri
        {
            get
            {
                string uri = $"{ConstSettings.AuthDomain}/cgi-bin/secstep";
                return uri;
            }
        }

        protected override byte[] CreateHttpContent()
        {
            string data = $"csrf={_csrf}&Login={Uri.EscapeUriString(_login)}&AuthCode={_authCode}&Permanent={(_doNotAskAgainForThisDevice ? 1 : 0)}";

            return Encoding.UTF8.GetBytes(data);
        }
    }
}


