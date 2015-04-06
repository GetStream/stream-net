using RestSharp;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamClient
    {
        internal const String BaseUrlFormat = "https://{0}-api.getstream.io";
        internal const String BaseUrlPath = "/api/v1.0/";

        readonly RestClient _client;
        readonly StreamClientOptions _options;
        readonly String _apiSecret;
        readonly String _apiKey;

        public StreamClient(String apiKey, String apiSecret, StreamClientOptions options = null)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (String.IsNullOrWhiteSpace(apiSecret))
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
        public StreamFeed Feed(String feedSlug, String userId)
        {
            String token = Sign(feedSlug + userId);
            return new StreamFeed(this, feedSlug, userId, token);
        }

        private String GetBaseUrl()
        {
            String region = "";
            switch(_options.Location)
            {
                //following the "specs", but ap-northeast seems to return 404 https://getstream.io/docs/#performance
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
            return String.Format(BaseUrlFormat, region);
        }

        internal RestSharp.RestRequest BuildRequest(StreamFeed feed, String path, Method method)
        {
            var request = new RestRequest(BaseUrlPath + feed.UrlPath + path, method);
            request.AddHeader("Authorization", feed.FeedTokenId + " " + feed.Token);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("api_key", _apiKey);
            request.Timeout = _options.Timeout;
            return request;
        }

        internal Task<IRestResponse> MakeRequest(RestRequest request)
        {
            return _client.ExecuteTaskAsync(request);
        }

        internal String Sign(String feedId) 
        {
            Encoding encoding = new ASCIIEncoding();
            var hashedSecret = (new SHA1Managed()).ComputeHash(encoding.GetBytes(_apiSecret));
            var hmac = new HMACSHA1(hashedSecret);
            return Convert.ToBase64String(hmac.ComputeHash(encoding.GetBytes(feedId)))
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Trim('=');
        }        

        internal String SignTo(String to)
        {
            String[] bits = to.Split(':');
            var otherFeed = this.Feed(bits[0], bits[1]);
            return to + " " + otherFeed.Token;
        }
    }
}
