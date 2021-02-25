using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stream
{
    public class Personalization
    {
        readonly StreamClient _client;

        internal Personalization(StreamClient client)
        {
            _client = client;
        }

        public async Task<IDictionary<string, object>> Get(string endpoint, IDictionary<string, object> data)
        {
            var request = this._client.BuildPersonalizationRequest(endpoint + "/", HttpMethod.GET);
            foreach (KeyValuePair<string, object> entry in data)
            {
                request.AddQueryParameter(entry.Key, Convert.ToString(entry.Value));
            }

            var response = await this._client.MakeRequest(request);
            if ((int)response.StatusCode < 300)
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<IDictionary<string, object>> Post(string endpoint, IDictionary<string, object> data)
        {
            var request = this._client.BuildPersonalizationRequest(endpoint + "/", HttpMethod.POST);
            request.SetJsonBody(JsonConvert.SerializeObject(data));

            var response = await this._client.MakeRequest(request);
            if ((int)response.StatusCode < 300)
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task Delete(string endpoint, IDictionary<string, object> data)
        {
            var request = this._client.BuildPersonalizationRequest(endpoint + "/", HttpMethod.DELETE);
            foreach (KeyValuePair<string, object> entry in data)
            {
                request.AddQueryParameter(entry.Key, Convert.ToString(entry.Value));
            }

            var response = await this._client.MakeRequest(request);
            if ((int)response.StatusCode >= 300)
                throw StreamException.FromResponse(response);
        }
    };
}
