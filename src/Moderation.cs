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
    public class Moderation : IModeration
    {
        private readonly StreamClient _client;

        public Moderation(StreamClient client)
        {
            _client = client;
        }

        public async Task<ResponseBase> FlagUserAsync(string flaggedUserID, string reason, IDictionary<string, object> options = null)
        {
            return await FlagAsync("stream:user", flaggedUserID, string.Empty, reason, options);
        }

        public async Task<ResponseBase> FlagActivityAsync(string entityId, string entityCreatorID, string reason, IDictionary<string, object> options = null)
        {
            return await FlagAsync("stream:feeds:v2:activity", entityId, entityCreatorID, reason, options);
        }

        public async Task<ResponseBase> FlagReactionAsync(string entityId, string entityCreatorID, string reason, IDictionary<string, object> options = null)
        {
            return await FlagAsync("stream:feeds:v2:reaction", entityId, entityCreatorID, reason, options);
        }

        public async Task<ResponseBase> FlagAsync(string entityType, string entityId, string entityCreatorID,
            string reason, IDictionary<string, object> options = null)
        {
            var request = _client.BuildAppRequest("moderation/flag", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new
            {
                user_id = entityCreatorID, entity_type = entityType, entity_id = entityId, reason,
            }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode > HttpStatusCode.Accepted)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }
    }
}