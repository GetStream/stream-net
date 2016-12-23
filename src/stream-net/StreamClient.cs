using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamClient
    {
        internal const string BaseUrlFormat = "https://{0}-api.getstream.io";
        internal const string BaseUrlPath = "/api/v1.0/";
        internal const string ActivitiesUrlPath = "activities/";
        internal const int ActivityCopyLimitDefault = 300;
        internal const int ActivityCopyLimitMax = 1000;

        readonly RestClient _client;
        readonly StreamClientOptions _options;
        readonly string _apiSecret;
        readonly string _apiKey;

        public StreamClient(string apiKey, string apiSecret, StreamClientOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (string.IsNullOrWhiteSpace(apiSecret))
                throw new ArgumentNullException("apiSecret", "Must have an apiSecret");

            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _options = options ?? StreamClientOptions.Default;
            _client = new RestClient(GetBaseUrl());
        }

        /// <summary>
        /// Get a feed
        /// </summary>
        /// <param name="feedSlug"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public StreamFeed Feed(string feedSlug, string userId)
        {
            // handle required arguments
            if (string.IsNullOrWhiteSpace(feedSlug))
                throw new ArgumentNullException("feedSlug", "Must have a feedSlug");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId", "Must have an userId");

            string token = Sign(feedSlug + userId);
            return new StreamFeed(this, feedSlug, userId, token);
        }

        /// <summary>
        /// Access batch operations
        /// </summary>
        public BatchOperations Batch
        {
            get
            {
                return new BatchOperations(this);
            }
        }

        private string GetBaseUrl()
        {
            string region = "";
            switch (_options.Location)
            {
                case StreamApiLocation.USEast:
                    region = "us-east";
                    break;
                case StreamApiLocation.USWest:
                    region = "us-west";
                    break;
                case StreamApiLocation.EUWest:
                    region = "eu-west";
                    break;
                case StreamApiLocation.AsiaJapan:
                    region = "ap-northeast";
                    break;
                default:
                    break;
            }
            return string.Format(BaseUrlFormat, region);
        }

        private RestSharp.RestRequest BuildRestRequest(string fullPath, Method method)
        {
            var request = new RestRequest(fullPath, method);
            request.AddHeader("Authorization", JWToken("*"));
            request.AddHeader("stream-auth-type", "jwt");
            request.AddQueryParameter("api_key", _apiKey);
            request.Timeout = _options.Timeout;
            return request;
        }

        internal RestSharp.RestRequest BuildFeedRequest(StreamFeed feed, string path, Method method)
        {
            return BuildRestRequest(BaseUrlPath + feed.UrlPath + path, method);
        }

        internal RestSharp.RestRequest BuildActivitiesRequest(StreamFeed feed)
        {
            return BuildRestRequest(BaseUrlPath + ActivitiesUrlPath, Method.POST);
        }

        internal RestSharp.RestRequest BuildAppRequest(string path, Method method)
        {
            var request = new RestRequest(BaseUrlPath + path, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-Api-Key", _apiKey);
            request.Timeout = _options.Timeout;
            return request;
        }

        internal void SignRequest(RestSharp.RestRequest request)
        {
            // make signature
            var queryString = "";
            request.Parameters.ForEach((p) =>
            {
                if (p.Type == ParameterType.QueryString)
                {
                    queryString += (queryString.Length == 0) ? "?" : "&";
                    queryString += string.Format("{0}={1}", p.Name, Uri.EscapeDataString(p.Value.ToString()));
                }
            });
            var toSign = string.Format("(request-target): {0} {1}", request.Method.ToString().ToLower(), request.Resource + queryString);

            var signature = string.Format("keyId=\"{0}\",algorithm=\"hmac-sha256\",headers=\"(request-target)\",signature=\"{1}\"", this._apiKey, Sign256(toSign));
            request.AddHeader("Authorization", "Signature " + signature);
        }

        internal Task<IRestResponse> MakeRequest(RestRequest request)
        {
            return _client.ExecuteTaskAsync(request);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Trim('=');
        }

        internal string Sign(string feedId)
        {
            Encoding encoding = new ASCIIEncoding();
            var hashedSecret = (new SHA1Managed()).ComputeHash(encoding.GetBytes(_apiSecret));
            var hmac = new HMACSHA1(hashedSecret);
            return Base64UrlEncode(hmac.ComputeHash(encoding.GetBytes(feedId)));
        }

        internal string Sign256(string feedId)
        {
            Encoding encoding = new ASCIIEncoding();
            var hmac = new HMACSHA256(encoding.GetBytes(_apiSecret));
            return Convert.ToBase64String(hmac.ComputeHash(encoding.GetBytes(feedId)));
        }

        internal string JWToken(string feedId)
        {
            var segments = new List<string>();
            var header = new {
                typ = "JWT",
                alg = "HS256" 
            };
            var noTimestamp = !this._options.ExpireTokens;
            var payload = new
            {
                resource = "*",
                action = "*",
                feed_id = feedId
            };

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));
            
            var stringToSign = string.Join(".", segments.ToArray());
            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            using (var sha = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret)))
            {
                byte[] signature = sha.ComputeHash(bytesToSign);
                segments.Add(Base64UrlEncode(signature));
            }            
            return string.Join(".", segments.ToArray());
        }

        internal string SignTo(string to)
        {
            string[] bits = to.Split(':');
            var otherFeed = this.Feed(bits[0], bits[1]);
            return to + " " + otherFeed.Token;
        }
    }
}
