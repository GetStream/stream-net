namespace Stream.Models
{
    public class FollowStat
    {
        public int Count { get; set; }
        public string Feed { get; set; }
    }

    public class FollowStats
    {
        public FollowStat Followers { get; set; }
        public FollowStat Following { get; set; }
    }

    public class FollowStatsResponse : ResponseBase
    {
        public FollowStats Results { get; set; }
    }
}


