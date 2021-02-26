using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamClient : IStreamClient
    {
        static readonly string Version = "4.1.1";

        internal const string BaseUrlFormat = "https://{0}-api.stream-io-api.com";
        internal const string BaseUrlPath = "/api/v1.0/";
        internal const string BasePersonalizationUrlFormat = "https://{0}-personalization.stream-io-api.com";
        internal const string BasePersonalizationUrlPath = "/personalization/v1.0/";
        internal const string ActivitiesUrlPath = "activities/";
        internal const string ImagesUrlPath = "images/";
        internal const string FilesUrlPath = "files/";

        internal const int ActivityCopyLimitDefault = 300;
        internal const int ActivityCopyLimitMax = 1000;

        internal static readonly object JWTHeader = new
        {
            typ = "JWT",
            alg = "HS256"
        };

        readonly RestClient _client;
        readonly StreamClientOptions _options;
        readonly IToken _streamClientToken;
        readonly string _apiKey;

        public StreamClient(string apiKey, string apiSecretOrToken, StreamClientOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (string.IsNullOrWhiteSpace(apiSecretOrToken))
                throw new ArgumentNullException("apiSecret", "Must have an apiSecret or user session token");

            _apiKey = apiKey;
            _streamClientToken = TokenFactory.For(apiSecretOrToken);
            _options = options ?? StreamClientOptions.Default;
            _client = new RestClient(GetBaseUrl(), TimeSpan.FromMilliseconds(_options.Timeout));
        }

        private StreamClient(string apiKey, IToken streamClientToken, RestClient client, StreamClientOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (streamClientToken is null)
                throw new ArgumentNullException("streamClientToken", "Must have a streamClientToken");

            _apiKey = apiKey;
            _streamClientToken = streamClientToken;
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

            return new StreamFeed(this, feedSlug, userId);
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

        public string CreateUserToken(string userId, IDictionary<string, object> extraData = null)
        {
            return _streamClientToken.CreateUserToken(userId, extraData);
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
                var _personalization = new RestClient(GetBasePersonalizationUrl(), TimeSpan.FromMilliseconds(_options.PersonalizationTimeout));
                return new Personalization(new StreamClient(_apiKey, _streamClientToken, _personalization, _options));
            }
        }

        public Files Files
        {
            get
            {
                return new Files(this);
            }
        }

        public Images Images
        {
            get
            {
                return new Images(this);
            }
        }

        private Uri GetBaseUrl()
        {
            return new Uri(string.Format(BaseUrlFormat, GetRegion(_options.Location)));
        }

        private Uri GetBasePersonalizationUrl()
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
            request.AddHeader("Stream-Auth-Type", "jwt");
            request.AddHeader("X-Stream-Client", "stream-net-" + Version);
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

        internal RestRequest BuildActivitiesRequest()
        {
            return BuildRestRequest(BaseUrlPath + ActivitiesUrlPath, HttpMethod.POST);
        }

        internal RestRequest BuildFileUploadRequest()
        {
            return BuildRestRequest(BaseUrlPath + FilesUrlPath, HttpMethod.POST);
        }

        internal RestRequest BuildImageUploadRequest()
        {
            return BuildRestRequest(BaseUrlPath + ImagesUrlPath, HttpMethod.POST);
        }

        internal RestRequest BuildAppRequest(string path, HttpMethod method)
        {
            return BuildRestRequest(BaseUrlPath + path, method);
        }

        internal RestRequest BuildPersonalizationRequest(string path, HttpMethod method)
        {
            return BuildRestRequest(BasePersonalizationUrlPath + path, method, "*");
        }

        internal Task<RestResponse> MakeRequest(RestRequest request)
        {
            return _client.Execute(request);
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
            return _streamClientToken.For(payload);
        }
    }
}
