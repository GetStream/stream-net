using Newtonsoft.Json;
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
        public void SetData(string name, object data) => Data.SetData(name, data);

        /// <summary>Sets a custom data value.</summary>
        public void SetData(IEnumerable<KeyValuePair<string, object>> data) => data.ForEach(x => Data.SetData(x.Key, x.Value));

        /// <summary>
        /// Sets a custom data value. If <paramref name="serializer"/> is not null, it will be used to serialize the value.
        /// </summary>
        public void SetData<T>(string name, T data, JsonSerializer serializer) => Data.SetData(name, data, serializer);

        /// <summary>
        /// Gets a custom data value parsed into <typeparamref name="T"/>.
        /// </summary>
        public T GetData<T>(string name) => Data.GetData<T>(name);
    }

    public class GetCollectionResponseObject
    {
        public string Id { get; set; }
        public string Collection { get; set; }
        public string ForeignId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CollectionObject Data { get; set; }
    }

    public class GetCollectionResponse
    {
        public List<GetCollectionResponseObject> Data { get; set; }
    }

    public class GetCollectionResponseWrap : ResponseBase
    {
        public GetCollectionResponse Response { get; set; }
    }
}
