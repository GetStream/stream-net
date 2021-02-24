using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stream
{
    public class AggregateActivity : Activity
    {
        public IList<Activity> Activities { get; internal set; }

        public int ActorCount { get; internal set; }

        public DateTime? CreatedAt { get; internal set; }

        public DateTime? UpdatedAt { get; internal set; }

        public string Group { get; internal set; }

        [JsonConstructor]
        internal AggregateActivity()
        {
        }
    }

    public class EnrichedAggregatedActivity : EnrichedActivity
    {
        public IList<EnrichedActivity> Activities { get; internal set; }

        public int ActorCount { get; internal set; }

        public DateTime? CreatedAt { get; internal set; }

        public DateTime? UpdatedAt { get; internal set; }

        public string Group { get; internal set; }

        [JsonConstructor]
        internal EnrichedAggregatedActivity()
        {
        }
    }
}
