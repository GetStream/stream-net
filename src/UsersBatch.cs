using Stream.Models;
using Stream.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class UsersBatch : IUsersBatch
    {
        private readonly StreamClient _client;

        internal UsersBatch(StreamClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<User>> UpsertUsersAsync(IEnumerable<User> users, bool overrideExisting = false)
        {
            var body = new Dictionary<string, object>
            {
                { "users", users },
                { "override", overrideExisting },
            };
            var request = _client.BuildAppRequest("users/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(body));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<IEnumerable<User>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds)
        {
            var request = _client.BuildAppRequest("users/", HttpMethod.Get);
            request.AddQueryParameter("ids", string.Join(",", userIds));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<IEnumerable<User>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<IEnumerable<string>> DeleteUsersAsync(IEnumerable<string> userIds)
        {
            var request = _client.BuildAppRequest("users/", HttpMethod.Delete);
            request.AddQueryParameter("ids", string.Join(",", userIds));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<IEnumerable<string>>(response.Content);

            throw StreamException.FromResponse(response);
        }
    }
}