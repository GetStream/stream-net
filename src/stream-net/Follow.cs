using Newtonsoft.Json;

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

        public Follow(IStreamFeed source, IStreamFeed target)
        {
            Source = source.FeedId;
            Target = target.FeedId;
        }
    }
}
