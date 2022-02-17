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
            var r = new Reaction
            {
                Kind = kind,
                ActivityId = activityId,
                UserId = userId,
                Data = data,
                TargetFeeds = targetFeeds,
            };

            return await AddAsync(r);
        }

        public async Task<Reaction> AddChild(Reaction parent, string kind, string userId,
            IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null)
        {
            var r = new Reaction()
            {
                Kind = kind,
                UserId = userId,
                Data = data,
                ParentId = parent.Id,
                TargetFeeds = targetFeeds,
            };

            return await AddAsync(r);
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
                return StreamJsonConverter.DeserializeObject<ReactionsFilterResponse>(response.Content).Reactions;
            }

            throw StreamException.FromResponse(response);
        }

        public async Task<ReactionsWithActivity> FilterWithActivityAsync(ReactionFiltering filtering, ReactionPagination pagination)
        {
            var response = await FilterInternalAsync(filtering.WithActivityData(), pagination);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var token = JToken.Parse(response.Content);
                var reactions = token.ToObject<ReactionsFilterResponse>().Reactions;
                var activity = token["activity"].ToObject<EnrichedActivity>();

                return new ReactionsWithActivity
                {
                    Reactions = reactions,
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
