using Newtonsoft.Json;
using System;

namespace GetStream
{
    public class Follower
    {
        [JsonProperty("feed_id")]
        public string FeedId { get; set; }

        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore()]
        [Obsolete("No longer a meaningful value according to GetStream. Returns the same value as CreatedAt and will be removed in a future release")]
        public DateTime UpdatedAt
        {
            get
            {
                return CreatedAt;
            }
        }

        [JsonConstructor]
        internal Follower()
        {
        }
    }
}
