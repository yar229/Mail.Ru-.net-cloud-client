﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MailRuCloudApi.Api.Requests
{
    public abstract class BaseRequest<T> where T : class
    {

        protected readonly CloudApi CloudApi;

        protected BaseRequest(CloudApi cloudApi)
        {
            CloudApi = cloudApi;
        }

        public virtual string RelationalUri { get; protected set; }

        public virtual HttpWebRequest CreateRequest(string baseDomain = null)
        {
            string domain = string.IsNullOrEmpty(baseDomain) ? ConstSettings.CloudDomain : baseDomain;
            var uriz = new Uri(new Uri(domain), RelationalUri);

            var request = (HttpWebRequest)WebRequest.Create(uriz);
            request.Proxy = CloudApi.Account.Proxy;
            request.CookieContainer = CloudApi.Account.Cookies;
            request.Method = "GET";
            request.ContentType = ConstSettings.DefaultRequestType;
            request.Accept = "application/json";
            request.UserAgent = ConstSettings.UserAgent;

            return request;
        }

        protected virtual byte[] CreateHttpContent()
        {
            return null;
        }


        public virtual async Task<T> MakeRequestAsync()
        {
            var httprequest = CreateRequest();

            var content = CreateHttpContent();
            if (content != null)
            {
                httprequest.Method = "POST";
                var stream = httprequest.GetRequestStream();
                stream.Write(content, 0, content.Length);
            }

            using (var response = (HttpWebResponse)await Task.Factory.FromAsync(httprequest.BeginGetResponse, asyncResult => httprequest.EndGetResponse(asyncResult), null))
            {

                if ((int)response.StatusCode >= 500)
                    throw new RequestException("Server fault") {StatusCode = response.StatusCode}; // Let's throw exception. It's server fault

                var responseText = ReadResponseAsText(response, CloudApi.CancelToken.Token); //await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = DeserializeMessage(responseText);
                if (!result.Ok || response.StatusCode != HttpStatusCode.OK)
                {
                    var exceptionMessage = $"Request failed (status code {(int)response.StatusCode}): {result.Description}";
                    throw new RequestException(exceptionMessage)
                    {
                        StatusCode = response.StatusCode,
                        ResponseBody = responseText,
                        Description = result.Description,
                        ErrorCode = result.ErrorCode
                    };
                }
                var retVal = result.Result;
                return retVal;
            }
        }

        protected string JsonSerialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }


        protected virtual RequestResponse<T> DeserializeMessage(string json)
        {

            var msg = new RequestResponse<T>
            {
                Ok = true,
                Result = typeof(T) == typeof(string) ? json as T : JsonConvert.DeserializeObject<T>(json)
            };
            return msg;
        }

        private static string ReadResponseAsText(WebResponse resp, CancellationToken token)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    ReadResponseAsByte(resp, token, stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
                catch
                {
                    //// Cancellation token.
                    return "7035ba55-7d63-4349-9f73-c454529d4b2e";
                }
            }
        }

        private static void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream = null)
        {
            int bufSizeChunk = 30000;
            int totalBufSize = bufSizeChunk;
            byte[] fileBytes = new byte[totalBufSize];

            int totalBytesRead = 0;

            var responseStream = resp.GetResponseStream();
            if (responseStream != null)
                using (var reader = new BinaryReader(responseStream))
                {
                    int bytesRead;
                    while ((bytesRead = reader.Read(fileBytes, totalBytesRead, totalBufSize - totalBytesRead)) > 0)
                    {
                        token.ThrowIfCancellationRequested();

                        outputStream?.Write(fileBytes, totalBytesRead, bytesRead);

                        totalBytesRead += bytesRead;

                        if (totalBufSize - totalBytesRead == 0)
                        {
                            totalBufSize += bufSizeChunk;
                            Array.Resize(ref fileBytes, totalBufSize);
                        }
                    }
                }
        }
    }
}
