using Stream.Models;
using Stream.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class Collections : ICollections
    {
        private readonly StreamClient _client;

        internal Collections(StreamClient client)
        {
            _client = client;
        }

        public async Task<ResponseBase> UpsertAsync(string collectionName, CollectionObject data)
        {
            return await UpsertManyAsync(collectionName, new[] { data });
        }

        public async Task<ResponseBase> UpsertManyAsync(string collectionName, IEnumerable<CollectionObject> data)
        {
            var body = new Dictionary<string, object>
            {
                {
                    "data", new Dictionary<string, IEnumerable<object>> { { collectionName, data.Select(x => x.Flatten()) } }
                },
            };
            var request = _client.BuildAppRequest("collections/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(body));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<CollectionObject> SelectAsync(string collectionName, string id)
        {
            var result = await SelectManyAsync(collectionName, new[] { id });
            return result.Response.Data.FirstOrDefault();
        }

        public async Task<GetCollectionResponseWrap> SelectManyAsync(string collectionName, IEnumerable<string> ids)
        {
            var request = _client.BuildAppRequest("collections/", HttpMethod.Get);
            request.AddQueryParameter("foreign_ids", string.Join(",", ids.Select(x => $"{collectionName}:{x}")));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<GetCollectionResponseWrap>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task DeleteManyAsync(string collectionName, IEnumerable<string> ids)
        {
            var request = _client.BuildAppRequest("collections/", HttpMethod.Delete);
            request.AddQueryParameter("collection_name", collectionName);
            request.AddQueryParameter("ids", string.Join(",", ids));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public async Task<CollectionObject> AddAsync(string collectionName, Dictionary<string, object> data, string id = null, string userId = null)
        {
            var collectionObject = new CollectionObject(id) { UserId = userId };
            data.ForEach(x => collectionObject.SetData(x.Key, x.Value));

            var request = _client.BuildAppRequest($"collections/{collectionName}/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(collectionObject));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.Created)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<CollectionObject>(response.Content);
        }

        public async Task<CollectionObject> GetAsync(string collectionName, string id)
        {
            var request = _client.BuildAppRequest($"collections/{collectionName}/{id}/", HttpMethod.Get);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<CollectionObject>(response.Content);
        }

        public async Task<CollectionObject> UpdateAsync(string collectionName, string id, Dictionary<string, object> data)
        {
            var request = _client.BuildAppRequest($"collections/{collectionName}/{id}/", HttpMethod.Put);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new { data = data }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.Created)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<CollectionObject>(response.Content);
        }

        public async Task<ResponseBase> DeleteAsync(string collectionName, string id)
        {
            var request = _client.BuildAppRequest($"collections/{collectionName}/{id}/", HttpMethod.Delete);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public string Ref(string collectionName, string collectionObjectId) => Ref(collectionName, new CollectionObject(collectionObjectId));
        public string Ref(string collectionName, CollectionObject obj) => obj.Ref(collectionName);
    }
}
