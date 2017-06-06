using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MailRuCloudApi.Api.Requests.Types;
using Newtonsoft.Json;

namespace MailRuCloudApi.Api.Requests
{
    class LoginRequest : BaseRequest<LoginResult>
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

        protected override RequestResponse<LoginResult> DeserializeMessage(string responseText)
        {

            LoginResult res = null;
            var match = Regex.Match(responseText, @"(?snx-)<script\s*type=""text/html""\sid=""json"">(?<data>.*?)</script>");
            if (match.Success)
            {
                res = JsonConvert.DeserializeObject<LoginResult>(match.Groups["data"].Value);
            }
            else res = new LoginResult();

            //TODO: implement captcha
            if (!string.IsNullOrEmpty(res.secstep_captcha))
                throw new NotImplementedException("Two-step auth captcha not implemented");

            var msg = new RequestResponse<LoginResult>
            {
                Ok = true,
                Result = res
            };
            return msg;
        }
    }
}
