namespace Stream.Models
{
    /// <summary>
    /// Represents a relationship to unfollow in batch operations
    /// </summary>
    public class UnfollowRelation
    {
        /// <summary>
        /// Source feed id
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Target feed id
        /// </summary>
        public string Target { get; set; }
        
        /// <summary>
        /// Whether to keep activities from the unfollowed feed
        /// </summary>
        public bool KeepHistory { get; set; }

        /// <summary>
        /// Creates a new instance of the UnfollowRelation class
        /// </summary>
        /// <param name="source">Source feed id</param>
        /// <param name="target">Target feed id</param>
        /// <param name="keepHistory">Whether to keep activities from the unfollowed feed</param>
        public UnfollowRelation(string source, string target, bool keepHistory = false)
        {
            Source = source;
            Target = target;
            KeepHistory = keepHistory;
        }

        /// <summary>
        /// Creates a new instance of the UnfollowRelation class
        /// </summary>
        /// <param name="source">Source feed</param>
        /// <param name="target">Target feed</param>
        /// <param name="keepHistory">Whether to keep activities from the unfollowed feed</param>
        public UnfollowRelation(IStreamFeed source, IStreamFeed target, bool keepHistory = false)
        {
            Source = source.FeedId;
            Target = target.FeedId;
            KeepHistory = keepHistory;
        }
    }
} 