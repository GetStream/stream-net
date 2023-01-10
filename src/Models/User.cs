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

        public static implicit operator User(string str) => new User { Id = str };

        public override bool Equals(Object obj)
        {
            if (obj is User)
                return this.Id == ((User)obj).Id;
            else if (obj is string)
                return this.Id == (string)obj;
            else
                return false;
        }
    }

    public class UserConverter : JsonConverter<User>
    {
        public override void WriteJson(JsonWriter writer, User value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Id);
        }

        public override User ReadJson(JsonReader reader, Type objectType, User existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (User)serializer.Deserialize<string>(reader);
        }
    }
}
