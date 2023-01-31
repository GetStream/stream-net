using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Stream.Utils
{
    public static class StreamJsonConverter
    {
        private static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fff",
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(), // this handles ForeignId => foreign_id etc. conversion for us
            },
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static JsonSerializer Serializer { get; } = JsonSerializer.Create(Settings);
        public static string SerializeObject(object obj) => JsonConvert.SerializeObject(obj, Settings);
        public static T DeserializeObject<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);
    }
}
