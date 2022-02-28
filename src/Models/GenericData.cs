using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Utils;
using System;

namespace Stream.Models
{
    [JsonConverter(typeof(GenericDataConverter))]
    public class GenericData : CustomDataBase
    {
        /// <summary>Identifier of the object.</summary>
        public string Id { get; set; }
    }

    internal class GenericDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var result = new GenericData();

            if (token.Type == JTokenType.String)
            {
                result.Id = token.Value<string>();
                return result;
            }

            if (token.Type == JTokenType.Object)
            {
                var obj = token as JObject;
                obj.Properties().ForEach(prop =>
                {
                    if (prop.Name == "id")
                        result.Id = prop.Value.Value<string>();
                    else
                        result.SetData(prop.Name, prop.Value);
                });

                return result;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}