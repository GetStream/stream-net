using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stream
{
    public class CollectionObject
    {
        public string ID { get; set; }
        readonly GenericData _data = new GenericData();

        internal CollectionObject() { }

        public CollectionObject(string id)
        {
            ID = id;
        }

        public T GetData<T>(string name)
        {
            return this._data.GetData<T>(name);
        }

        public void SetData<T>(string name, T data)
        {
            this._data.SetData<T>(name, data);
        }

        internal JObject ToJObject()
        {
            var root = new JObject();
            root.Add(new JProperty("id", this.ID));
            this._data.AddToJObject(ref root);
            return root;
        }

        internal static CollectionObject FromJSON(JObject obj)
        {
            CollectionObject result = new CollectionObject();

            obj.Properties().ForEach(prop =>
            {
                switch (prop.Name)
                {
                    case "id": result.ID = prop.Value.Value<string>(); break;
                    default: result._data.SetData(prop.Name, prop.Value); break;
                }
            });

            return result;
        }
    }

    public class Collections
    {
        readonly StreamClient _client;

        internal Collections(StreamClient client)
        {
            _client = client;
        }

        public async Task Upsert(string collectionName, CollectionObject data)
        {
            await this.UpsertMany(collectionName, new CollectionObject[] { data });
        }

        public async Task UpsertMany(string collectionName, IEnumerable<CollectionObject> data)
        {
            var dataJson = new JObject(
                new JProperty("data", new JObject(
                    new JProperty(collectionName, data.Select(x => x.ToJObject())))));

            var request = this._client.BuildJWTAppRequest("meta/", HttpMethod.POST);
            request.SetJsonBody(dataJson.ToString());

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task<CollectionObject> Select(string collectionName, string id)
        {
            var result = await this.SelectMany(collectionName, new string[] { id });
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<CollectionObject>> SelectMany(string collectionName, IEnumerable<string> ids)
        {
            var foreignIds = ids.Select(x => string.Format("{0}:{1}", collectionName, x));

            var request = this._client.BuildJWTAppRequest("meta/", HttpMethod.GET);
            request.AddQueryParameter("foreign_ids", string.Join(",", foreignIds));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Collections.GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task Delete(string collectionName, string id)
        {
            await this.DeleteMany(collectionName, new string[] { id });
        }

        public async Task DeleteMany(string collectionName, IEnumerable<string> ids)
        {
            var request = this._client.BuildJWTAppRequest("meta/", HttpMethod.DELETE);
            request.AddQueryParameter("collection_name", collectionName);
            request.AddQueryParameter("ids", string.Join(",", ids));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public static string Ref(string collectionName, CollectionObject obj)
        {
            return string.Format("SO:{0}:{1}", collectionName, obj.ID);
        }

        private static IEnumerable<CollectionObject> GetResults(string json)
        {
            var obj = JObject.Parse(json);
            var response = obj.Property("response").Value as JObject;
            var data = response.Property("data").Value as JArray;

            foreach (var result in data)
            {
                var resultObj = result as JObject;

                var foreignID = resultObj.Property("foreign_id").Value.Value<string>();
                var objectID = foreignID.Split(':')[1];

                var objectData = resultObj.Property("data").Value as JObject;

                var collectionObject = CollectionObject.FromJSON(objectData);
                collectionObject.ID = objectID;

                yield return collectionObject;
            }
        }
    }
}
