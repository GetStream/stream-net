using Newtonsoft.Json.Linq;
using Stream.Models;
using Stream.Rest;
using Stream.Utils;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class Reactions : IReactions
    {
        private readonly StreamClient _client;

        internal Reactions(StreamClient client)
        {
            _client = client;
        }

        public async Task<Reaction> AddAsync(string kind, string activityId, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            return await AddAsync(null, kind, activityId, userId, data, targetFeeds);
        }

        public async Task<Reaction> AddAsync(string reactionId, string kind, string activityId, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction
            {
                Id = reactionId,
                Kind = kind,
                ActivityId = activityId,
                UserId = userId,
                Data = data,
                TargetFeeds = targetFeeds,
            };

            return await AddAsync(r);
        }

        public async Task<Reaction> AddChildAsync(Reaction parent, string kind, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            return await AddChildAsync(parent.Id, null, kind, userId, data, targetFeeds);
        }

        public async Task<Reaction> AddChildAsync(string parentId, string kind, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            return await AddChildAsync(parentId, null, kind, userId, data, targetFeeds);
        }

        public async Task<Reaction> AddChildAsync(string parentId, string reactionId, string kind, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                Id = reactionId,
                Kind = kind,
                UserId = userId,
                Data = data,
                ParentId = parentId,
                TargetFeeds = targetFeeds,
            };

            return await AddAsync(r);
        }

        public async Task<Reaction> AddChildAsync(Reaction parent, string reactionId, string kind, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            return await AddChildAsync(parent.Id, reactionId, kind, userId, data, targetFeeds);
        }

        public async Task<Reaction> GetAsync(string reactionId)
        {
            var request = _client.BuildAppRequest($"reaction/{reactionId}/", HttpMethod.Get);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<IEnumerable<Reaction>> FilterAsync(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var response = await FilterInternalAsync(filtering, pagination);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return StreamJsonConverter.DeserializeObject<ReactionsFilterResponse>(response.Content).Results;
            }

            throw StreamException.FromResponse(response);
        }

        public async Task<ReactionsWithActivity> FilterWithActivityAsync(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var response = await FilterInternalAsync(filtering.WithActivityData(), pagination);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var token = JToken.Parse(response.Content);
                var reactions = token.ToObject<ReactionsFilterResponse>().Results;
                var activity = token["activity"].ToObject<EnrichedActivity>();

                return new ReactionsWithActivity
                {
                    Results = reactions,
                    Activity = activity,
                };
            }

            throw StreamException.FromResponse(response);
        }

        private async Task<RestResponse> FilterInternalAsync(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var urlPath = pagination.GetPath();
            var request = _client.BuildAppRequest($"reaction/{urlPath}", HttpMethod.Get);
            filtering.Apply(request);

            var response = await _client.MakeRequestAsync(request);

            return response;
        }

        public async Task<Reaction> UpdateAsync(string reactionId, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction
            {
                Id = reactionId,
                Data = data,
                TargetFeeds = targetFeeds,
            };

            var request = _client.BuildAppRequest($"reaction/{reactionId}/", HttpMethod.Put);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(r));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task DeleteAsync(string reactionId)
        {
            var request = _client.BuildAppRequest($"reaction/{reactionId}/", HttpMethod.Delete);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        private async Task<Reaction> AddAsync(Reaction r)
        {
            var request = _client.BuildAppRequest("reaction/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(r));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<Reaction>(response.Content);

            throw StreamException.FromResponse(response);
        }
    }
}
