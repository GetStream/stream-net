using Newtonsoft.Json;

namespace Stream.Models
{
    public class Follow
    {
        public string Source { get; set; }
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
