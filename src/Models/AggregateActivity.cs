using System;
using System.Collections.Generic;

namespace Stream.Models
{
    public abstract class AggregateActivityBase
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string Verb { get; set; }
        public int ActorCount { get; set; }
        public int ActivityCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AggregateActivity : AggregateActivityBase
    {
        public List<Activity> Activities { get; set; }
    }

    public class EnrichedAggregateActivity : AggregateActivityBase
    {
        public List<EnrichedActivity> Activities { get; set; }
        public int Score { get; set; }
    }
}
