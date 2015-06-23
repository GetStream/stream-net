using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stream
{
    public class AggregateActivity : Activity
    {
        public IList<Activity> Activities { get; internal set; }

        public int ActorCount { get; internal set; }

        [JsonConstructor]
        internal AggregateActivity()
        {
        }
    }
}
