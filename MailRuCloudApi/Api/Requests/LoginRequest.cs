using System;
using System.Net;
using System.Text;

namespace MailRuCloudApi.Api.Requests
{
    class LoginRequest : BaseRequest<string>
    {
        private readonly string _login;
        private readonly string _password;

        public LoginRequest(CloudApi cloudApi, string login, string password) : base(cloudApi)
        {
            _login = login;
            _password = password;
        }

        public override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(ConstSettings.AuthDomain);
            request.Accept = ConstSettings.DefaultAcceptType;
            return request;
        }

        public override string RelationalUri => "/cgi-bin/auth";

        protected override byte[] CreateHttpContent()
        {
            string data = $"Login={Uri.EscapeUriString(_login)}&Domain={ConstSettings.Domain}&Password={Uri.EscapeUriString(_password)}";

            return Encoding.UTF8.GetBytes(data);
        }
    }
}
