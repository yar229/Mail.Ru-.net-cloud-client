using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Requests;
using MailRuCloudApi.Api.Streams;
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


        public byte[] GetFile(File file)
        {
            var stream = new DownloadStream(file, this, null, null, null);
            //using (MemoryStream ms = new MemoryStream())
            //{ 
            //    stream.CopyTo(ms);
            //    return ms.ToArray();
            //}

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
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
