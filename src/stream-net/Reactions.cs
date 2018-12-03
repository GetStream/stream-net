using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stream
{
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

    public class Reactions
    {
        readonly StreamClient _client;

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

        public async Task<Reaction> AddChild(string parentID, string kind, string activityID, string userID,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                Kind = kind,
                ActivityID = activityID,
                UserID = userID,
                Data = data,
                ParentID = parentID,
                TargetFeeds = targetFeeds
            };

            return await this.Add(r);
        }

        public async Task<Reaction> Get(string reactionID)
        {
            var request = this._client.BuildJWTAppRequest($"reaction/{reactionID}/", HttpMethod.GET);

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<Reaction> Update(string reactionID, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                ID = reactionID,
                Data = data,
                TargetFeeds = targetFeeds
            };

            var request = this._client.BuildJWTAppRequest($"reaction/{reactionID}/", HttpMethod.PUT);
            request.SetJsonBody(JsonConvert.SerializeObject(r));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return JsonConvert.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);

        }

        public async Task Delete(string reactionID)
        {
            var request = this._client.BuildJWTAppRequest($"reaction/{reactionID}/", HttpMethod.DELETE);

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        private async Task<Reaction> Add(Reaction r)
        {
            var request = this._client.BuildJWTAppRequest("reaction/", HttpMethod.POST);
            request.SetJsonBody(JsonConvert.SerializeObject(r));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return JsonConvert.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }

    }
}
