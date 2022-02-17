using System.Collections.Generic;

namespace Stream.Models
{
    public class EnrichedActivity : ActivityBase
    {
        public GenericData Actor { get; set; }
        public GenericData Object { get; set; }
        public GenericData Target { get; set; }
        public GenericData Origin { get; set; }
        public Dictionary<string, int> ReactionCounts { get; set; }
        public Dictionary<string, List<Reaction>> OwnReactions { get; set; }
        public Dictionary<string, List<Reaction>> LatestReactions { get; set; }
    }
}
