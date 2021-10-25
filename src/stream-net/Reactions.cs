using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Stream
{
    using ReactionFilter = FeedFilter;

    public class ReactionsWithActivity
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "results")]
        public IEnumerable<Reaction> Reactions { get; internal set; }

        public EnrichedActivity Activity { get; internal set; }
    }

    public class Reaction
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "id")]
        public string ID { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "kind")]
        public string Kind { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "created_at")]
        public DateTime? CreatedAt { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "activity_id")]
        public string ActivityID { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "user_id")]
        public string UserID { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "user"), JsonConverter(typeof(EnrichableFieldConverter))]
        public EnrichableField User { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "data")]
        public IDictionary<string, object> Data { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "target_feeds")]
        public IEnumerable<string> TargetFeeds { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "parent")]
        public string ParentID { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "latest_children")]
        public Dictionary<string, Reaction[]> LatestChildren { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "children_counts")]
        public Dictionary<string, int> ChildrenCounters { get; internal set; }

    }

    public class ReactionFiltering
    {
        const int DefaultLimit = 10;
        int _limit = DefaultLimit;
        ReactionFilter _filter = null;

        public ReactionFiltering WithLimit(int limit)
        {
            _limit = limit;
            return this;
        }

        public ReactionFiltering WithFilter(ReactionFilter filter)
        {
            _filter = filter;
            return this;
        }

        internal ReactionFiltering WithActivityData()
        {
            _filter = (_filter == null) ? ReactionFilter.Where().WithActivityData() : _filter.WithActivityData();

            return this;
        }

        internal void Apply(RestRequest request)
        {
            request.AddQueryParameter("limit", _limit.ToString());

            // filter if needed
            if (_filter != null)
                _filter.Apply(request);
        }

        internal bool IncludesActivityData
        {
            get
            {
                return _filter.IncludesActivityData;
            }
        }

        public static ReactionFiltering Default
        {
            get
            {
                return new ReactionFiltering()
                {
                    _limit = DefaultLimit
                };
            }
        }
    }

    public class ReactionPagination
    {
        string _kind = null;
        string _lookup_attr;
        string _lookup_value;

        private ReactionPagination() { }

        public ReactionPagination ActivityID(string activityID)
        {
            _lookup_attr = "activity_id";
            _lookup_value = activityID;
            return this;
        }

        public ReactionPagination ReactionID(string reactionID)
        {
            _lookup_attr = "reaction_id";
            _lookup_value = reactionID;
            return this;
        }

        public ReactionPagination UserID(string userID)
        {
            _lookup_attr = "user_id";
            _lookup_value = userID;
            return this;
        }

        public ReactionPagination Kind(string kind)
        {
            _kind = kind;
            return this;
        }

        public static ReactionPagination By
        {
            get { return new ReactionPagination(); }
        }

        public string GetPath()
        {
            if (_kind != null)
                return $"{_lookup_attr}/{_lookup_value}/{_kind}/";
            return $"{_lookup_attr}/{_lookup_value}/";
        }
    }

    public class Reactions
    {
        readonly StreamClient _client;

        private class ReactionsFilterResponse
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "results")]
            public IEnumerable<Reaction> Reactions { get; internal set; }

            internal static EnrichedActivity GetActivity(string json)
            {
                JObject obj = JObject.Parse(json);
                foreach (var prop in obj.Properties())
                {
                    if (prop.Name == "activity")
                    {
                        return EnrichedActivity.FromJson((JObject)prop.Value);
                    }
                }

                return null;
            }
        }

        internal Reactions(StreamClient client)
        {
            _client = client;
        }

        public async Task<Reaction> Add(string kind, string activityID, string userID,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                Kind = kind,
                ActivityID = activityID,
                UserID = userID,
                Data = data,
                TargetFeeds = targetFeeds
            };

            return await this.Add(r);
        }

        public async Task<Reaction> AddChild(string parentID, string kind, string userID,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                Kind = kind,
                UserID = userID,
                Data = data,
                ParentID = parentID,
                TargetFeeds = targetFeeds
            };

            return await this.Add(r);
        }


        public async Task<Reaction> Get(string reactionID)
        {
            var request = this._client.BuildAppRequest($"reaction/{reactionID}/", HttpMethod.GET);

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<IEnumerable<Reaction>> Filter(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var response = await FilterHelper(filtering, pagination);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ReactionsFilterResponse>(response.Content).Reactions;
            }

            throw StreamException.FromResponse(response);
        }

        public async Task<ReactionsWithActivity> FilterWithActivityData(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var response = await FilterHelper(filtering.WithActivityData(), pagination);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var reactions = JsonConvert.DeserializeObject<ReactionsFilterResponse>(response.Content).Reactions;
                var activity = ReactionsFilterResponse.GetActivity(response.Content);

                return new ReactionsWithActivity
                {
                    Reactions = reactions,
                    Activity = activity
                };
            }

            throw StreamException.FromResponse(response);
        }

        private async Task<RestResponse> FilterHelper(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var urlPath = pagination.GetPath();
            var request = this._client.BuildAppRequest($"reaction/{urlPath}", HttpMethod.GET);
            filtering.Apply(request);

            var response = await this._client.MakeRequest(request);

            return response;
        }

        public async Task<Reaction> Update(string reactionID, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                ID = reactionID,
                Data = data,
                TargetFeeds = targetFeeds
            };

            var request = this._client.BuildAppRequest($"reaction/{reactionID}/", HttpMethod.PUT);
            request.SetJsonBody(JsonConvert.SerializeObject(r));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return JsonConvert.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);

        }

        public async Task Delete(string reactionID)
        {
            var request = this._client.BuildAppRequest($"reaction/{reactionID}/", HttpMethod.DELETE);

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        private async Task<Reaction> Add(Reaction r)
        {
            var request = this._client.BuildAppRequest("reaction/", HttpMethod.POST);
            request.SetJsonBody(JsonConvert.SerializeObject(r));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return JsonConvert.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }

    }
}
