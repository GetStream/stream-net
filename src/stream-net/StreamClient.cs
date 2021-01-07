﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamClient : IStreamClient
    {
        internal const string BaseUrlFormat = "https://{0}-api.stream-io-api.com";
        internal const string BaseUrlPath = "/api/v1.0/";
        internal const string BasePersonalizationUrlFormat = "https://{0}-personalization.stream-io-api.com";
        internal const string BasePersonalizationUrlPath = "/personalization/v1.0/";
        internal const string ActivitiesUrlPath = "activities/";
        internal const int ActivityCopyLimitDefault = 300;
        internal const int ActivityCopyLimitMax = 1000;

        internal static readonly object JWTHeader = new
        {
            typ = "JWT",
            alg = "HS256"
        };

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
            _client = new RestClient(GetBaseUrl(_options.Location), TimeSpan.FromMilliseconds(_options.Timeout));
        }

        private StreamClient(string apiKey, string apiSecret, RestClient client, StreamClientOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (string.IsNullOrWhiteSpace(apiSecret))
                throw new ArgumentNullException("apiSecret", "Must have an apiSecret");

            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _options = options ?? StreamClientOptions.Default;
            _client = client;
        }


        /// <summary>
        /// Get a feed
        /// </summary>
        /// <param name="feedSlug"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IStreamFeed Feed(string feedSlug, string userId)
        {
            // handle required arguments
            if (string.IsNullOrWhiteSpace(feedSlug))
                throw new ArgumentNullException("feedSlug", "Must have a feedSlug");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId", "Must have an userId");

            string token = Sign(feedSlug + userId);
            return new StreamFeed(this, feedSlug, userId, token);
        }

        public async Task ActivityPartialUpdate(string id = null, ForeignIDTime foreignIDTime = null, GenericData set = null, IEnumerable<string> unset = null)
        {
            if (id == null && foreignIDTime == null)
                throw new ArgumentException("one the parameters ids or foreignIdTimes must be provided and not null", "ids, foreignIDTimes");
            if (id != null && foreignIDTime != null)
                throw new ArgumentException("at most one of the parameters ids or foreignIdTimes must be provided", "ids, foreignIDTimes");

            var update = new ActivityPartialUpdateRequestObject
            {
                ID = id,
                ForeignIDTime = foreignIDTime,
                Set = set,
                Unset = unset,
            };
            await this.Batch.ActivitiesPartialUpdate(new ActivityPartialUpdateRequestObject[] { update });
        }

        public string CreateUserSessionToken(string userId, IDictionary<string, object> extraData = null)
        {
            var payload = new Dictionary<string, object>
            {
                {"user_id", userId}
            };
            if (extraData != null)
            {
                extraData.ForEach(x => payload[x.Key] = x.Value);
            }
            return this.JWToken(payload);
        }

        /// <summary>
        /// Access batch operations
        /// </summary>
        public IBatchOperations Batch
        {
            get
            {
                return new BatchOperations(this);
            }
        }

        public Collections Collections
        {
            get
            {
                return new Collections(this);
            }
        }

        public Reactions Reactions
        {
            get
            {
                return new Reactions(this);
            }
        }

        public Users Users
        {
            get
            {
                return new Users(this);
            }
        }

        public Personalization Personalization
        {
            get
            {
                var _personalization = new RestClient(GetBasePersonalizationUrl(_options.PersonalizationLocation), TimeSpan.FromMilliseconds(_options.PersonalizationTimeout));
                return new Personalization(new StreamClient(_apiKey, _apiSecret, _personalization, _options));
            }
        }

        private Uri GetBaseUrl(StreamApiLocation location)
        {
            return new Uri(string.Format(BaseUrlFormat, GetRegion(_options.Location)));
        }

        private Uri GetBasePersonalizationUrl(StreamApiLocation location)
        {
            return new Uri(string.Format(BasePersonalizationUrlFormat, GetRegion(_options.PersonalizationLocation)));
        }

        private string GetRegion(StreamApiLocation location)
        {
            switch (location)
            {
                case StreamApiLocation.USEast:
                    return "us-east";
                case StreamApiLocation.Tokyo:
                    return "tokyo";
                case StreamApiLocation.Dublin:
                    return "dublin";
                case StreamApiLocation.Singapore:
                    return "singapore";
                default:
                    return "us-east";
            }
        }

        private RestRequest BuildRestRequest(string fullPath, HttpMethod method, string userID = null)
        {
            var request = new RestRequest(fullPath, method);
            request.AddHeader("Authorization", JWToken("*", userID));
            request.AddHeader("stream-auth-type", "jwt");
            request.AddQueryParameter("api_key", _apiKey);
            return request;
        }

        internal RestRequest BuildFeedRequest(StreamFeed feed, string path, HttpMethod method)
        {
            return BuildRestRequest(BaseUrlPath + feed.UrlPath + path, method);
        }

        internal RestRequest BuildEnrichedFeedRequest(StreamFeed feed, string path, HttpMethod method)
        {
            return BuildRestRequest(BaseUrlPath + feed.EnrichedPath + path, method);
        }

        internal RestRequest BuildActivitiesRequest(StreamFeed feed)
        {
            return BuildRestRequest(BaseUrlPath + ActivitiesUrlPath, HttpMethod.POST);
        }

        internal RestRequest BuildJWTAppRequest(string path, HttpMethod method)
        {
            return BuildRestRequest(BaseUrlPath + path, method);
        }

        internal RestRequest BuildAppRequest(string path, HttpMethod method)
        {
            var request = new RestRequest(BaseUrlPath + path, method);
            request.AddHeader("X-Api-Key", _apiKey);
            return request;
        }

        internal RestRequest BuildPersonalizationRequest(string path, HttpMethod method)
        {
            return BuildRestRequest(BasePersonalizationUrlPath + path, method, "*");
        }

        internal void SignRequest(RestRequest request)
        {
            // make signature
            var queryString = "";
            request.QueryParameters.ForEach((p) =>
            {
                queryString += (queryString.Length == 0) ? "?" : "&";
                queryString += string.Format("{0}={1}", p.Key, Uri.EscapeDataString(p.Value.ToString()));
            });
            var toSign = string.Format("(request-target): {0} {1}", request.Method.ToString().ToLower(), request.Resource + queryString);

            var signature = string.Format("keyId=\"{0}\",algorithm=\"hmac-sha256\",headers=\"(request-target)\",signature=\"{1}\"", this._apiKey, Sign256(toSign));
            request.AddHeader("Authorization", "Signature " + signature);
        }

        internal Task<RestResponse> MakeRequest(RestRequest request)
        {
            return _client.Execute(request);
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
#if NETCORE
            var hashedSecret = SHA1.Create().ComputeHash(encoding.GetBytes(_apiSecret));
#else
            var hashedSecret = (new SHA1Managed()).ComputeHash(encoding.GetBytes(_apiSecret));
#endif

            var hmac = new HMACSHA1(hashedSecret);
            return Base64UrlEncode(hmac.ComputeHash(encoding.GetBytes(feedId)));
        }

        internal string Sign256(string feedId)
        {
            Encoding encoding = new ASCIIEncoding();
            var hmac = new HMACSHA256(encoding.GetBytes(_apiSecret));
            return Convert.ToBase64String(hmac.ComputeHash(encoding.GetBytes(feedId)));
        }

        internal string JWToken(string feedId, string userID = null)
        {
            var payload = new Dictionary<string, string>()
            {
                {"resource", "*"},
                {"action", "*"},
                {"feed_id", feedId}
            };
            if (userID != null)
            {
                payload["user_id"] = userID;
            }
            return this.JWToken(payload);
        }

        internal string JWToken(object payload)
        {
            var segments = new List<string>();

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(StreamClient.JWTHeader));
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
