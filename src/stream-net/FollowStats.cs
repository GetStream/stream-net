using Newtonsoft.Json;

namespace Stream
{

    public class FollowStat
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("feed")]
        public string Feed { get; set; }
    }

    public class FollowStats
    {

        [JsonProperty("followers")]
        public FollowStat Followers { get; set; }

        [JsonProperty("following")]
        public FollowStat Following { get; set; }
    }
}


