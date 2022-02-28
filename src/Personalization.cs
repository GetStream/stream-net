using Newtonsoft.Json;
using Stream.Models;
using Stream.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class Personalization : IPersonalization
    {
        private readonly StreamClient _client;

        internal Personalization(StreamClient client)
        {
            _client = client;
        }

        public async Task<IDictionary<string, object>> GetAsync(string endpoint, IDictionary<string, object> data)
        {
            var request = _client.BuildPersonalizationRequest(endpoint + "/", HttpMethod.Get);
            foreach (KeyValuePair<string, object> entry in data)
            {
                request.AddQueryParameter(entry.Key, Convert.ToString(entry.Value));
            }

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<Dictionary<string, object>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<IDictionary<string, object>> PostAsync(string endpoint, IDictionary<string, object> data)
        {
            var request = _client.BuildPersonalizationRequest(endpoint + "/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(data));

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<Dictionary<string, object>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> DeleteAsync(string endpoint, IDictionary<string, object> data)
        {
            var request = _client.BuildPersonalizationRequest(endpoint + "/", HttpMethod.Delete);
            foreach (KeyValuePair<string, object> entry in data)
            {
                request.AddQueryParameter(entry.Key, Convert.ToString(entry.Value));
            }

            var response = await _client.MakeRequestAsync(request);

            if ((int)response.StatusCode >= 300)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);
        }
    }
}
