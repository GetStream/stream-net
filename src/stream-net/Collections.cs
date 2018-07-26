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
        public string Name { get; set; }

        readonly IDictionary<string, JToken> _data = new Dictionary<string, JToken>();

        internal CollectionObject(){}

        public CollectionObject(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public T GetData<T>(string name)
        {
            JToken val;
            return this._data.TryGetValue(name, out val) ? val.ToObject<T>() : default(T);
        }

        public void SetData<T>(string name, T data)
        {
            this._data[name] = JValue.FromObject(data);
        }

        internal JObject ToJObject()
        {
            var root = new JObject();
            root.Add(new JProperty("id", this.ID));
            root.Add(new JProperty("name", this.Name));
            this._data.ForEach( x => root.Add(x.Key, x.Value));
            return root;
        }

        internal static CollectionObject FromJSON(JObject obj)
        {
            CollectionObject result = new CollectionObject();

            obj.Properties().ForEach(prop => {
                switch(prop.Name)
                {
                    case "id": result.ID = prop.Value.Value<string>(); break;
                    case "name": result.Name = prop.Value.Value<string>(); break;
                    default: result._data[prop.Name] = prop.Value; break;
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
            await this.UpsertMany(collectionName, new List<CollectionObject>{data});
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

        private static IEnumerable<CollectionObject> GetResults(string json)
        {
            var obj = JObject.Parse(json);
            var data = obj.Property("data").Value as JObject;

            var values = data.PropertyValues().Select(x => x as JArray);
            foreach (var value in values)
            {
                foreach (var res in value)
                    yield return CollectionObject.FromJSON((JObject)res);

            }
        }
    }
}