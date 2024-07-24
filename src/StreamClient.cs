using Stream.Models;
using Stream.Rest;
using Stream.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>
    /// Can be used for basic operations and for instantiating clients that can be used to interact with the API.
    /// <para>This and all returned clients can be used as a singleton as they do not store state.</para>
    /// </summary>
    public class StreamClient : IStreamClient
    {
        private const string BaseUrlFormat = "https://{0}-api.stream-io-api.com";
        private const string BaseUrlPath = "/api/v1.0/";
        private const string BasePersonalizationUrlFormat = "https://{0}-personalization.stream-io-api.com";
        private const string BasePersonalizationUrlPath = "/personalization/v1.0/";
        private const string ActivitiesUrlPath = "activities/";
        private const string ImagesUrlPath = "images/";
        private const string FilesUrlPath = "files/";
        internal const int ActivityCopyLimitDefault = 300;
        internal const int ActivityCopyLimitMax = 1000;
        private readonly string Version;
        private readonly RestClient _client;
        private readonly StreamClientOptions _options;
        private readonly IToken _streamClientToken;
        private readonly string _apiKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamClient"/> class.
        /// </summary>
        /// <param name="apiKey">The API key of a Stream application.</param>
        /// <param name="apiSecretOrToken">The API secret or token of a Stream application.</param>
        /// <exception cref="ArgumentNullException">If API key or API secret/token is null.</exception>
        public StreamClient(string apiKey, string apiSecretOrToken) : this(apiKey, apiSecretOrToken, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamClient"/> class.
        /// </summary>
        /// <param name="apiKey">The API key of a Stream application.</param>
        /// <param name="apiSecretOrToken">The API secret or token of a Stream application.</param>
        /// <param name="options">Additional options to configure the underlying HTTP client.</param>
        /// <exception cref="ArgumentNullException">If API key or API secret/token is null.</exception>
        public StreamClient(string apiKey, string apiSecretOrToken, StreamClientOptions options)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (string.IsNullOrWhiteSpace(apiSecretOrToken))
                throw new ArgumentNullException("apiSecret", "Must have an apiSecret or user session token");

            _apiKey = apiKey;
            _streamClientToken = TokenFactory.For(apiSecretOrToken);
            _options = options ?? StreamClientOptions.Default;
            var stream_url = Environment.GetEnvironmentVariable("STREAM_URL");
            var url = new Uri(
                stream_url == null || stream_url.Length == 0 ?
                string.Format(BaseUrlFormat, GetRegion(_options.Location)) :
                stream_url, UriKind.Absolute);

            _client = new RestClient(url, TimeSpan.FromMilliseconds(_options.Timeout));

            var assemblyVersion = typeof(StreamClient).GetTypeInfo().Assembly.GetName().Version;
            Version = assemblyVersion.ToString(3);

            Batch = new BatchOperations(this);
            Collections = new Collections(this);
            Reactions = new Reactions(this);
            Users = new Users(this);
            Moderation = new Moderation(this);
            var personalization = new RestClient(GetBasePersonalizationUrl(), TimeSpan.FromMilliseconds(_options.PersonalizationTimeout));
            Personalization = new Personalization(new StreamClient(_apiKey, _streamClientToken, personalization, _options));
            Files = new Files(this);
            Images = new Images(this);
        }

        private StreamClient(string apiKey, IToken streamClientToken, RestClient client, StreamClientOptions options = null)
        {
            _apiKey = apiKey;
            _streamClientToken = streamClientToken;
            _options = options ?? StreamClientOptions.Default;
            _client = client;
        }

        public IBatchOperations Batch { get; }
        public ICollections Collections { get; }
        public IReactions Reactions { get; }
        public IUsers Users { get; }
        public IPersonalization Personalization { get; }
        public IFiles Files { get; }
        public IImages Images { get; }
        public IModeration Moderation { get; }

        public IStreamFeed Feed(string feedSlug, string userId)
        {
            if (string.IsNullOrWhiteSpace(feedSlug))
                throw new ArgumentNullException(nameof(feedSlug), $"Must have a {nameof(feedSlug)}");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), $"Must have a {nameof(userId)}");

            return new StreamFeed(this, feedSlug, userId);
        }

        public async Task<PersonalizedGetResponse<EnrichedActivity>> GetPersonalizedFeedAsync(GetOptions options = null)
        {
            options = options ?? GetOptions.Default;
            var request = this.BuildPersonalizedFeedRequest();
            options.Apply(request);

            var response = await this.MakeRequestAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<PersonalizedGetResponse<EnrichedActivity>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> ActivityPartialUpdateAsync(string id = null, ForeignIdTime foreignIdTime = null, Dictionary<string, object> set = null, IEnumerable<string> unset = null)
        {
            if (id == null && foreignIdTime == null)
                throw new ArgumentException("one the parameters ids or foreignIdTimes must be provided and not null", $"{nameof(id)}, {nameof(foreignIdTime)}");
            if (id != null && foreignIdTime != null)
                throw new ArgumentException("at most one of the parameters ids or foreignIdTimes must be provided", $"{nameof(id)}, {nameof(foreignIdTime)}");

            var update = new ActivityPartialUpdateRequestObject
            {
                Id = id,
                ForeignId = foreignIdTime?.ForeignId,
                Set = set,
                Time = foreignIdTime?.Time,
                Unset = unset,
            };

            return await this.Batch.ActivitiesPartialUpdateAsync(new[] { update });
        }

        public async Task<Og> OgAsync(string url)
        {
            var request = this.BuildAppRequest("og/", HttpMethod.Get);
            request.AddQueryParameter("url", url);

            var response = await this.MakeRequestAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<Og>(response.Content);
        }

        public string CreateUserToken(string userId, IDictionary<string, object> extraData = null)
        {
            return _streamClientToken.CreateUserToken(userId, extraData);
        }

        private Uri GetBasePersonalizationUrl()
        {
            var stream_url = Environment.GetEnvironmentVariable("STREAM_URL");
            return new Uri(
                stream_url == null || stream_url.Length == 0 ?
                string.Format(BasePersonalizationUrlFormat, GetRegion(_options.PersonalizationLocation)) :
                stream_url, UriKind.Absolute);
        }

        private static string GetRegion(StreamApiLocation location)
        {
            switch (location)
            {
                case StreamApiLocation.USEast:
                    return "us-east";
                case StreamApiLocation.Dublin:
                    return "dublin";
                case StreamApiLocation.Tokyo:
                    return "tokyo";
                case StreamApiLocation.Mumbai:
                    return "mumbai";
                case StreamApiLocation.Singapore:
                    return "singapore";
                case StreamApiLocation.Sidney:
                    return "sydney";
                case StreamApiLocation.Oregon:
                    return "oregon";
                case StreamApiLocation.Ohio:
                    return "ohio";
                default:
                    return "us-east";
            }
        }

        private RestRequest BuildRestRequest(string fullPath, HttpMethod method, string userId = null)
        {
            var request = new RestRequest(fullPath, method);
            request.AddHeader("Authorization", JWToken("*", userId));
            request.AddHeader("Stream-Auth-Type", "jwt");
            request.AddHeader("X-Stream-Client", "stream-net-" + Version);
            request.AddQueryParameter("api_key", _apiKey);
            return request;
        }

        internal RestRequest BuildFeedRequest(StreamFeed feed, string path, HttpMethod method)
            => BuildRestRequest(BaseUrlPath + feed.UrlPath + path, method);

        internal RestRequest BuildEnrichedFeedRequest(StreamFeed feed, string path, HttpMethod method)
            => BuildRestRequest(BaseUrlPath + feed.EnrichedPath + path, method);

        internal RestRequest BuildPersonalizedFeedRequest()
            => BuildRestRequest(BaseUrlPath + "enrich/personalization/feed/", HttpMethod.Get);

        internal RestRequest BuildActivitiesRequest()
            => BuildRestRequest(BaseUrlPath + ActivitiesUrlPath, HttpMethod.Post);

        internal RestRequest BuildFileUploadRequest()
            => BuildRestRequest(BaseUrlPath + FilesUrlPath, HttpMethod.Post);

        internal RestRequest BuildImageUploadRequest()
            => BuildRestRequest(BaseUrlPath + ImagesUrlPath, HttpMethod.Post);

        internal RestRequest BuildAppRequest(string path, HttpMethod method)
            => BuildRestRequest(BaseUrlPath + path, method);

        internal RestRequest BuildPersonalizationRequest(string path, HttpMethod method)
            => BuildRestRequest(BasePersonalizationUrlPath + path, method, "*");

        internal async Task<RestResponse> MakeRequestAsync(RestRequest request)
            => await _client.ExecuteHttpRequestAsync(request);

        internal string JWToken(string feedId, string userId = null)
        {
            var payload = new Dictionary<string, string>
            {
                { "resource", "*" },
                { "action", "*" },
                { "feed_id", feedId },
            };

            if (userId != null)
            {
                payload["user_id"] = userId;
            }

            return _streamClientToken.For(payload);
        }
    }
}
