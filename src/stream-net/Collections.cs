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
        public string UserID { get; set; }
        internal GenericData _data = new GenericData();

        public CollectionObject() { }

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
            if (this.ID != null)
            {
                root.Add(new JProperty("id", this.ID));
            }
            if (this.UserID != null)
            {
                root.Add(new JProperty("user_id", this.UserID));
            }
            this._data.AddToJObject(ref root);
            return root;
        }

        internal string ToJson()
        {
            var root = new JObject();
            if (this.ID != null)
            {
                root.Add(new JProperty("id", this.ID));
            }
            if (this.UserID != null)
            {
                root.Add(new JProperty("user_id", this.UserID));
            }
            root.Add(new JProperty("data", this._data.ToJObject()));
            return root.ToString();
        }

        internal static CollectionObject FromBatchJSON(JObject obj)
        {
            CollectionObject result = new CollectionObject();

            obj.Properties().ForEach(prop =>
            {
                switch (prop.Name)
                {
                    case "id": result.ID = prop.Value.Value<string>(); break;
                    case "user_id": result.UserID = prop.Value.Value<string>(); break;
                    default: result._data.SetData(prop.Name, prop.Value); break;
                }
            });

            return result;
        }

        internal static CollectionObject FromJSON(JObject obj)
        {
            CollectionObject result = new CollectionObject();

            obj.Properties().ForEach(prop =>
            {
                switch (prop.Name)
                {
                    case "id": result.ID = prop.Value.Value<string>(); break;
                    case "user_id": result.UserID = prop.Value.Value<string>(); break;
                    case "data":
                        {
                            var dataObj = prop.Value as JObject;
                            dataObj.Properties().ForEach(p =>
                            {
                                result.SetData(p.Name, p.Value);
                            });
                            break;
                        }
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

            var request = this._client.BuildJWTAppRequest("collections/", HttpMethod.POST);
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

            var request = this._client.BuildJWTAppRequest("collections/", HttpMethod.GET);
            request.AddQueryParameter("foreign_ids", string.Join(",", foreignIds));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Collections.GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task DeleteMany(string collectionName, IEnumerable<string> ids)
        {
            var request = this._client.BuildJWTAppRequest("collections/", HttpMethod.DELETE);
            request.AddQueryParameter("collection_name", collectionName);
            request.AddQueryParameter("ids", string.Join(",", ids));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public async Task<CollectionObject> Add(string collectionName, GenericData data, string ID = null, string userID = null)
        {
            var collectionObject = new CollectionObject()
            {
                ID = ID,
                UserID = userID,
                _data = data,
            };

            var request = this._client.BuildJWTAppRequest($"collections/{collectionName}/", HttpMethod.POST);
            request.SetJsonBody(collectionObject.ToJson());

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);

            return CollectionObject.FromJSON(JObject.Parse(response.Content));
        }

        public async Task<CollectionObject> Get(string collectionName, string ID)
        {
            var request = this._client.BuildJWTAppRequest($"collections/{collectionName}/{ID}/", HttpMethod.GET);

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return CollectionObject.FromJSON(JObject.Parse(response.Content));
        }

        public async Task<CollectionObject> Update(string collectionName, string ID, GenericData data)
        {
            var dataJson = new JObject(new JProperty("data", data.ToJObject()));
            var request = this._client.BuildJWTAppRequest($"collections/{collectionName}/{ID}/", HttpMethod.PUT);
            request.SetJsonBody(dataJson.ToString());

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);

            return CollectionObject.FromJSON(JObject.Parse(response.Content));
        }

        public async Task Delete(string collectionName, string ID)
        {
            var request = this._client.BuildJWTAppRequest($"collections/{collectionName}/{ID}/", HttpMethod.DELETE);

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public static string Ref(string collectionName, string collectionObjectID)
        {
            return string.Format("SO:{0}:{1}", collectionName, collectionObjectID);
        }

        public static string Ref(string collectionName, CollectionObject obj)
        {
            return Ref(collectionName, obj.ID);
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

                var collectionObject = CollectionObject.FromBatchJSON(objectData);
                collectionObject.ID = objectID;

                yield return collectionObject;
            }
        }
    }
}
