using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Requests;
using MailRuCloudApi.Extensions;

namespace MailRuCloudApi.Api
{
    public class CloudApi
    {

        /// <summary>
        /// Async tasks cancelation token.
        /// </summary>
        public readonly CancellationTokenSource CancelToken = new CancellationTokenSource();


        /// <summary>
        /// Gets or sets account to connect with cloud.
        /// </summary>
        /// <value>Account info.</value>
        public Account Account { get; set; }


        public CloudApi(string login, string password)
        {
            Account = new Account(this, login, password);
            if (!Account.Login())
            {
                throw new AuthenticationException("Auth token has't been retrieved.");
            }

            // !!!!!!!!!!!!!!!! Account.Info = GetAccountInfo().Result;
        }


        private async Task<object> GetFile(string sourceFile, string fileName, long contentLength = 0)
        {
            var shard = await GetShardInfo(ShardType.Get);
            MemoryStream memoryStream = new MemoryStream();


            var request = (HttpWebRequest)WebRequest.Create($"{shard.Url}{sourceFile.TrimStart('/')}");
            request.Proxy = Account.Proxy;
            request.CookieContainer = Account.Cookies;
            request.Method = "GET";
            request.ContentType = ConstSettings.DefaultRequestType;
            request.Accept = ConstSettings.DefaultAcceptType;
            request.UserAgent = ConstSettings.UserAgent;
            request.AllowReadStreamBuffering = false;
            var task = Task.Factory.FromAsync(request.BeginGetResponse, asyncResult => request.EndGetResponse(asyncResult), null);
            await task.ContinueWith(
                (t, m) =>
                {
                    var token = (CancellationToken)m;

                        try
                        {
                            ReadResponseAsByte(t.Result, token, memoryStream, contentLength, OperationType.Download);
                            return memoryStream.ToArray() as object;
                        }
                        catch
                        {
                            return null;
                        }
                      
                },
            CancelToken.Token);
            

            var result = memoryStream.ToArray() as object;

            memoryStream.Dispose();
            memoryStream.Close();

            return result;
        }



        /// <summary>
        /// Get shard info that to do post get request. Can be use for anonymous user.
        /// </summary>
        /// <param name="shardType">Shard type as numeric type.</param>
        /// <param name="useAnonymousUser">To get anonymous user.</param>
        /// <returns>Shard info.</returns>
        public async Task<ShardInfo> GetShardInfo(ShardType shardType, bool useAnonymousUser = false)
        {
            var data = await new ShardInfoRequest(this, useAnonymousUser).MakeRequestAsync();
            var shard = data.ToShardInfo(shardType);
            return shard;
        }

    }






}
