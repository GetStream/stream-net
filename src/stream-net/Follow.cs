using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class Follow
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        public Follow(string source, string target)
        {
            Source = source;
            Target = target;
        }

        public Follow(StreamFeed source, StreamFeed target)
        {
            Source = source.FeedId;
            Target = target.FeedId;
        }
    }
}
