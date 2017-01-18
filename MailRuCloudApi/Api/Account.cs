using System;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Requests;

namespace MailRuCloudApi.Api
{
    /// <summary>
    /// MAIL.RU account info.
    /// </summary>
    public class Account
    {
        private readonly CloudApi _cloudApi;

        /// <summary>
        /// Default cookies.
        /// </summary>
        private CookieContainer _cookies;

        /// <summary>
        /// Initializes a new instance of the <see cref="Account" /> class.
        /// </summary>
        /// <param name="cloudApi"></param>
        /// <param name="login">Login name as email.</param>
        /// <param name="password">Password related with this login</param>
        public Account(CloudApi cloudApi, string login, string password)
        {
            _cloudApi = cloudApi;
            LoginName = login;
            Password = password;
        }

        /// <summary>
        /// Gets or sets connection proxy.
        /// </summary>
        /// <value>Proxy settings.</value>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Gets authorization token.
        /// </summary>
        /// <value>Access token.</value>
        public string AuthToken { get; private set; }

        /// <summary>
        /// Gets account cookies.
        /// </summary>
        /// <value>Account cookies.</value>
        public CookieContainer Cookies => _cookies ?? (_cookies = new CookieContainer());

        /// <summary>
        /// Gets or sets login name.
        /// </summary>
        /// <value>Account email.</value>
        public string LoginName { get; set; }

        /// <summary>
        /// Gets or sets email password.
        /// </summary>
        /// <value>Password related with login.</value>
        public string Password { get; set; }

        public AccountInfo Info { get; set; }

        /// <summary>
        /// Authorize on MAIL.RU server.
        /// </summary>
        /// <returns>True or false result operation.</returns>
        public bool Login()
        {
            return this.LoginAsync().Result;
        }

        /// <summary>
        /// Async call to authorize on MAIL.RU server.
        /// </summary>
        /// <returns>True or false result operation.</returns>
        public async Task<bool> LoginAsync()
        {
            if (string.IsNullOrEmpty(LoginName))
            {
                throw new ArgumentException("LoginName is null or empty.");
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentException("Password is null or empty.");
            }

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            Proxy = WebRequest.DefaultWebProxy;


            string reqString = $"Login={LoginName}&Domain={ConstSettings.Domain}&Password={Password}";
            byte[] requestData = Encoding.UTF8.GetBytes(reqString);
            var request = (HttpWebRequest)WebRequest.Create($"{ConstSettings.AuthDomen}/cgi-bin/auth");
            request.Proxy = Proxy;
            request.CookieContainer = Cookies;
            request.Method = "POST";
            request.ContentType = ConstSettings.DefaultRequestType;
            request.Accept = ConstSettings.DefaultAcceptType;
            request.UserAgent = ConstSettings.UserAgent;
            var task = Task.Factory.FromAsync(request.BeginGetRequestStream, asyncResult => request.EndGetRequestStream(asyncResult), null);
            return await await task.ContinueWith(async (t) =>
            {
                using (var s = t.Result)
                {
                    s.Write(requestData, 0, requestData.Length);
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception();
                        }

                        if (this.Cookies != null && this.Cookies.Count > 0)
                        {
                            await EnsureSdcCookie();
                            var token = await GetAuthToken();

                            var accdata = await new AccountInfoRequest(_cloudApi).MakeRequestAsync();
                            Info = new AccountInfo
                            {
                                FileSizeLimit = accdata.body.cloud.file_size_limit
                            };

                            return token;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Need to add this function for all calls.
        /// </summary>
        internal void CheckAuth()
        {
            if (LoginName == null || Password == null)
                throw new AuthenticationException("Login or password is empty.");

            if (string.IsNullOrEmpty(AuthToken))
                if (!Login())
                    throw new AuthenticationException("Auth token has't been retrieved.");
        }

        /// <summary>
        /// Retrieve SDC cookies.
        /// </summary>
        /// <returns>Returns nothing. Just tusk.</returns>
        private async Task EnsureSdcCookie()
        {
            //var request = new EnsureSdcCookieRequest(_cloud);
            //await _cloud.MakeRequestAsync(request);

            var request = (HttpWebRequest)WebRequest.Create($"{ConstSettings.AuthDomen}/sdc?from={ConstSettings.CloudDomain}/home");
            request.Proxy = Proxy;
            request.CookieContainer = Cookies;
            request.Method = "GET";
            request.ContentType = ConstSettings.DefaultRequestType;
            request.Accept = ConstSettings.DefaultAcceptType;
            request.UserAgent = ConstSettings.UserAgent;
            var task = Task.Factory.FromAsync(request.BeginGetResponse, asyncResult => request.EndGetResponse(asyncResult), null);
            await task.ContinueWith((t) =>
            {
                using (var response = t.Result as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception();
                    }
                }
            });
        }

        /// <summary>
        /// Get authorization token.
        /// </summary>
        /// <returns>True or false result operation.</returns>
        private async Task<bool> GetAuthToken()
        {
            var data = await new AuthTokenRequest(_cloudApi).MakeRequestAsync();
            if (string.IsNullOrEmpty(data?.body?.token))
                throw new AuthenticationException("Empty auth token");
            AuthToken = data.body.token;

            return true;
        }



    }
}
