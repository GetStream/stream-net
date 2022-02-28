using Stream.Utils;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class Users : IUsers
    {
        private readonly StreamClient _client;

        internal Users(StreamClient client)
        {
            _client = client;
        }

        public async Task<User> AddAsync(string userId, IDictionary<string, object> data = null, bool getOrCreate = false)
        {
            var u = new User
            {
                Id = userId,
                Data = data,
            };
            var request = _client.BuildAppRequest("user/", HttpMethod.Post);

            request.SetJsonBody(StreamJsonConverter.SerializeObject(u));
            request.AddQueryParameter("get_or_create", getOrCreate.ToString());

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<User>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<User> GetAsync(string userId)
        {
            var request = _client.BuildAppRequest($"user/{userId}/", HttpMethod.Get);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<User>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<User> UpdateAsync(string userId, IDictionary<string, object> data)
        {
            var u = new User
            {
                Data = data,
            };
            var request = _client.BuildAppRequest($"user/{userId}/", HttpMethod.Put);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(u));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<User>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task DeleteAsync(string userId)
        {
            var request = _client.BuildAppRequest($"user/{userId}/", HttpMethod.Delete);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public string Ref(string userId) => Ref(new User { Id = userId });
        public string Ref(User obj) => obj.Ref();
    }
}
