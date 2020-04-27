using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stream
{

    public class EnrichableField
    {
        private object _data;

        public GenericData Enriched
        {
            get
            {
                return _data as GenericData;
            }
            set { }
        }

        public bool IsEnriched
        {
            get
            {
                return _data is GenericData;
            }
            set { }
        }
        public string Raw
        {
            get
            {
                return _data as string;
            }
            set { }
        }

        private EnrichableField() { }

        internal static EnrichableField FromJSON(JToken token)
        {
            var result = new EnrichableField();
            if (token.Type == JTokenType.String)
                result._data = token.Value<string>();
            else if (token.Type == JTokenType.Object)
            {
                var data = new GenericData();
                var obj = token as JObject;
                obj.Properties().ForEach(prop => data.SetData(prop.Name, prop.Value));
                result._data = data;
            }
            return result;
        }


    };


    public class EnrichableFieldConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return EnrichableField.FromJSON(JToken.Load(reader));
        }
    }
}
