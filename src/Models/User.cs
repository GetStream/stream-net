using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Stream
{
    public class User
    {
        public string Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IDictionary<string, object> Data { get; set; }

        /// <summary>Returns a reference identifier to this object.</summary>
        public string Ref() => $"SU:{Id}";

        public override string ToString() => Id;
    }

    public class UserConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(User))
            {
                var user = (User)value;
                writer.WriteValue(user.Id);
                return;
            }

            writer.WriteValue((string)value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var id = serializer.Deserialize<string>(reader);
            Console.WriteLine($"ReadJson id: {id}");

            return new User { Id = id };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(User) || objectType == typeof(string);
        }
    }
}
