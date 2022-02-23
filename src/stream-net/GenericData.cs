using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stream
{
    public class GenericData
    {
        readonly IDictionary<string, JToken> _data = new Dictionary<string, JToken>();

        public GenericData() { }

        public T GetData<T>(string name)
        {
            JToken val;
            return this._data.TryGetValue(name, out val) ? val.ToObject<T>() : default(T);
        }

        public void SetData<T>(string name, T data) => SetData(name, data, null);

        public void SetData(IEnumerable<KeyValuePair<string, object>> data) => data.ForEach(x => SetData(x.Key, x.Value, null));

        public void SetData<T>(string name, T data, JsonSerializer serializer)
        {
            if (serializer != null)
                _data[name] = JValue.FromObject(data, serializer);
            else
                _data[name] = JValue.FromObject(data);
        }

        internal JObject ToJObject()
        {
            var root = new JObject();
            this.AddToJObject(ref root);
            return root;
        }

        internal void AddToJObject(ref JObject root)
        {
            var tmp = root;
            this._data.ForEach(x => tmp.Add(x.Key, x.Value));
            root = tmp;
        }
    }
}
