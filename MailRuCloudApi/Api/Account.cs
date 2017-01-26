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

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            Proxy = WebRequest.DefaultWebProxy;
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
            return LoginAsync().Result;
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

            await new LoginRequest(_cloudApi, LoginName, Password)
                .MakeRequestAsync();

            await new EnsureSdcCookieRequest(_cloudApi)
                .MakeRequestAsync();

            AuthToken = new AuthTokenRequest(_cloudApi)
                .MakeRequestAsync()
                .ThrowIf(data => string.IsNullOrEmpty(data.body?.token), new AuthenticationException("Empty auth token"))
                .body.token;

            Info = new AccountInfo
            {
                FileSizeLimit = new AccountInfoRequest(_cloudApi).MakeRequestAsync().Result.body.cloud.file_size_limit
            };

            Expires = DateTime.Now.AddHours(23);

            return true;
        }

        public DateTime Expires { get; private set; }

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

    }

    public static class Extensions
    {
        public static T ThrowIf<T>(this Task<T> data, Func<T, bool> func, Exception ex)
        {
            var res = data.Result;
            if (func(res)) throw ex;
            return res;
        }
    }
}
