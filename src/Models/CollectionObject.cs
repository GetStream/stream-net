using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Utils;
using System;
using System.Collections.Generic;

namespace Stream.Models
{
    public class CollectionObject
    {
        public CollectionObject(string id) => Id = id;

        public string Id { get; set; }
        public string UserId { get; set; }
        public GenericData Data { get; set; } = new GenericData();

        /// <summary>Returns a reference identifier to this object.</summary>
        public string Ref(string collectionName) => $"SO:{collectionName}:{Id}";

        /// <summary>Sets a custom data value.</summary>
        public void SetData<T>(string name, T data) => Data.SetData(name, data, null);

        /// <summary>Sets multiple custom data.</summary>
        public void SetData(IEnumerable<KeyValuePair<string, object>> data) => data.ForEach(x => SetData(x.Key, x.Value, null));

        /// <summary>
        /// Sets a custom data value. If <paramref name="serializer"/> is not null, it will be used to serialize the value.
        /// </summary>
        public void SetData<T>(string name, T data, JsonSerializer serializer) => Data.SetData(name, data, serializer);

        /// <summary>
        /// Gets a custom data value parsed into <typeparamref name="T"/>.
        /// </summary>
        public T GetData<T>(string name) => Data.GetData<T>(name);

        internal JObject Flatten()
        {
            var flat = new JObject();

            if (!string.IsNullOrWhiteSpace(Id))
                flat["id"] = Id;

            if (!string.IsNullOrEmpty(UserId))
                flat["user_id"] = UserId;

            if (Data?.GetAllData()?.Count > 0)
            {
                foreach (var kvp in Data.GetAllData())
                    flat[kvp.Key] = kvp.Value;
            }

            return flat;
        }
    }

    public class GetCollectionResponse
    {
        public List<CollectionObject> Data { get; set; }
    }

    public class GetCollectionResponseWrap : ResponseBase
    {
        public GetCollectionResponse Response { get; set; }
    }
}
