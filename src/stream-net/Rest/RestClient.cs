using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stream.Rest
{
    internal class RestClient
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly Uri _baseUrl;
        private TimeSpan _timeout;

        public RestClient(Uri baseUrl, TimeSpan timeout)
        {
            _baseUrl = baseUrl;
            _timeout = timeout;
        }

        private HttpRequestMessage BuildRequestMessage(System.Net.Http.HttpMethod method, Uri url, RestRequest request)
        {
#if OLD_TLS_HANDLING
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
            var requestMessage = new HttpRequestMessage(method, url);
            requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // add request headers
            request.Headers.ForEach(h =>
            {
                requestMessage.Headers.Add(h.Key, h.Value);
            });

            if (request.FileStream != null)
            {
                requestMessage.Content = this.CreateFileStream(request);
            }
            else if (method == System.Net.Http.HttpMethod.Post || method == System.Net.Http.HttpMethod.Put)
            {
                requestMessage.Content = new StringContent(request.JsonBody, Encoding.UTF8, "application/json");
            }

            return requestMessage;
        }

        private HttpContent CreateFileStream(RestRequest request)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(request.FileStream);

            if (request.FileStreamContentType != null)
            {
                streamContent.Headers.Add("Content-Type", request.FileStreamContentType);
            }

            streamContent.Headers.Add("Content-Disposition", "form-data; name=\"file\"; filename=\"" + request.FileStreamName + "\"");
            content.Add(streamContent);
            return content;
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private async Task<RestResponse> ExecuteHttpRequest(System.Net.Http.HttpMethod method, Uri url, RestRequest request)
        {
            var requestMessage = BuildRequestMessage(method, url, request);
            using (var cts = new CancellationTokenSource(_timeout))
            {
                var response = await _client.SendAsync(requestMessage, cts.Token);
                return await RestResponse.FromResponseMessage(response);
            }
        }

        private async Task<RestResponse> ExecuteGet(Uri url, RestRequest request)
        {
            return await ExecuteHttpRequest(System.Net.Http.HttpMethod.Get, url, request);
        }

        private async Task<RestResponse> ExecutePost(Uri url, RestRequest request)
        {
            return await ExecuteHttpRequest(System.Net.Http.HttpMethod.Post, url, request);
        }

        private async Task<RestResponse> ExecutePut(Uri url, RestRequest request)
        {
            return await ExecuteHttpRequest(System.Net.Http.HttpMethod.Put, url, request);
        }

        private async Task<RestResponse> ExecuteDelete(Uri url, RestRequest request)
        {
            return await ExecuteHttpRequest(System.Net.Http.HttpMethod.Delete, url, request);
        }

        private Uri BuildUri(RestRequest request)
        {
            var queryString = "";
            request.QueryParameters.ForEach((p) =>
            {
                queryString += (queryString.Length == 0) ? "?" : "&";
                queryString += string.Format("{0}={1}", p.Key, Uri.EscapeDataString(p.Value.ToString()));
            });
            return new Uri(_baseUrl, request.Resource + queryString);
        }

        public Task<RestResponse> Execute(RestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request", "Request is required");

            Uri url = this.BuildUri(request);

            switch (request.Method)
            {
                case HttpMethod.DELETE:
                    return this.ExecuteDelete(url, request);
                case HttpMethod.POST:
                    return this.ExecutePost(url, request);
                case HttpMethod.PUT:
                    return this.ExecutePut(url, request);
                default:
                    return this.ExecuteGet(url, request);
            }
        }
    }
}
