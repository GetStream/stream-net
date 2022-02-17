using System;

namespace Stream.Models
{
    public class Follower
    {
        public string FeedId { get; set; }
        public string TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
