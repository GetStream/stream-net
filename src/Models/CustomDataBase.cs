using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Utils;
using System.Collections.Generic;

namespace Stream.Models
{
    public abstract class CustomDataBase
    {
        [JsonExtensionData]
        protected virtual Dictionary<string, JToken> Data { get; set; } = new Dictionary<string, JToken>();

        /// <summary>
        /// Returns all custom data
        /// </summary>
        public Dictionary<string, JToken> GetAllData() => Data;

        /// <summary>
        /// Gets a custom data value parsed into <typeparamref name="T"/>
        /// </summary>
        public T GetData<T>(string name) => GetDataInternal<T>(name);

        /// <summary>
        /// Gets a custom data value parsed into <typeparamref name="T"/>.
        /// If it doesn't exist, it returns <paramref name="default"/>.
        /// </summary>
        public T GetDataOrDefault<T>(string name, T @default) => Data.TryGetValue(name, out var val) ? val.ToObject<T>() : @default;

        private T GetDataInternal<T>(string name)
        {
            if (Data.TryGetValue(name, out var val))
            {
                // Hack logic:
                // Sometimes our customers provide raw json strings instead of objects.
                // So for example:
                // SetData<string>("stringcomplex", "{ \"test1\": 1, \"test2\": \"testing\" }");
                // instead of
                // SetData<string>("stringcomplex", new Dictionary<string, object> { { "test1", 1 }, { "test2", "testing" } });
                if (val.Type == JTokenType.String && val.Value<string>().StartsWith("{") && val.Value<string>().EndsWith("}"))
                {
                    return StreamJsonConverter.DeserializeObject<T>(val.Value<string>());
                }

                return val.ToObject<T>();
            }

            return default(T);
        }

        /// <summary>Sets a custom data value.</summary>
        public void SetData<T>(string name, T data) => SetData(name, data, null);

        /// <summary>Sets multiple custom data.</summary>
        public void SetData(IEnumerable<KeyValuePair<string, object>> data) => data.ForEach(x => SetData(x.Key, x.Value, null));

        /// <summary>
        /// Sets a custom data value. If <paramref name="serializer"/> is not null, it will be used to serialize the value.
        /// </summary>
        public void SetData<T>(string name, T data, JsonSerializer serializer)
        {
            if (serializer != null)
                Data[name] = JValue.FromObject(data, serializer);
            else
                Data[name] = JValue.FromObject(data);
        }
    }
}