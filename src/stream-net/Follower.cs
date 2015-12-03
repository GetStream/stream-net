using Newtonsoft.Json;
using System;

namespace Stream
{
    public class Follower
    {
        [JsonProperty("feed_id")]
        public string FeedId { get; set; }

        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonConstructor]
        internal Follower()
        {
        }
    }
}
