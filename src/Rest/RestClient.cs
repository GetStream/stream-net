using Stream.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stream.Rest
{
    internal class RestClient
    {
        private static readonly MediaTypeWithQualityHeaderValue _jsonAcceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
        private static readonly HttpClient _client = new HttpClient();
        private readonly Uri _baseUrl;
        private readonly TimeSpan _timeout;

        internal RestClient(Uri baseUrl, TimeSpan timeout)
        {
#if OLD_TLS_HANDLING
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
            _baseUrl = baseUrl;
            _timeout = timeout;
        }

        private HttpRequestMessage BuildRequestMessage(HttpMethod method, Uri url, RestRequest request)
        {
            var requestMessage = new HttpRequestMessage(method, url);
            requestMessage.Headers.Accept.Add(_jsonAcceptHeader);

            request.Headers.ForEach(h =>
            {
                requestMessage.Headers.Add(h.Key, h.Value);
            });

            if (request.FileStream != null)
            {
                requestMessage.Content = CreateFileStream(request);
            }
            else if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                requestMessage.Content = new StringContent(request.JsonBody ?? "{}", Encoding.UTF8, "application/json");
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

        private Uri BuildUri(RestRequest request)
        {
            var queryStringBuilder = new StringBuilder();
            request.QueryParameters.ForEach(p =>
            {
                queryStringBuilder.Append(queryStringBuilder.Length == 0 ? "?" : "&");
                queryStringBuilder.Append($"{p.Key}={Uri.EscapeDataString(p.Value)}");
            });

            return new Uri(_baseUrl, request.Resource + queryStringBuilder.ToString());
        }

        internal async Task<RestResponse> ExecuteHttpRequestAsync(RestRequest request)
        {
            var uri = BuildUri(request);

            using (var cts = new CancellationTokenSource(_timeout))
            using (var requestMessage = BuildRequestMessage(request.Method, uri, request))
            {
                var response = await _client.SendAsync(requestMessage, cts.Token);
                return await RestResponse.FromResponseMessage(response);
            }
        }
    }
}
