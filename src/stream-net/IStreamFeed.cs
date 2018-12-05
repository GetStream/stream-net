using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    public interface IStreamFeed
    {
        string FeedId { get; }
        string ReadOnlyToken { get; }
        string Token { get; }
        string UrlPath { get; }

        Task<IEnumerable<Activity>> AddActivities(IEnumerable<Activity> activities);
        Task<Activity> AddActivity(Activity activity);
        Task<IEnumerable<Follower>> Followers(int offset = 0, int limit = 25, string[] filterBy = null);
        Task FollowFeed(IStreamFeed feedToFollow, int activityCopyLimit = 300);
        Task FollowFeed(string targetFeedSlug, string targetUserId, int activityCopyLimit = 300);
        Task<IEnumerable<Follower>> Following(int offset = 0, int limit = 25, string[] filterBy = null);
        Task<IEnumerable<Activity>> GetActivities(int offset = 0, int limit = 20, FeedFilter filter = null, ActivityMarker marker = null);
        Task<StreamResponse<AggregateActivity>> GetAggregateActivities(GetOptions options = null);
        Task<StreamResponse<Activity>> GetFlatActivities(GetOptions options = null);
        Task<StreamResponse<NotificationActivity>> GetNotificationActivities(GetOptions options = null);
        Task<StreamResponse<EnrichedAggregatedActivity>> GetEnrichedAggregatedActivities(GetOptions options = null);
        Task<StreamResponse<EnrichedActivity>> GetEnrichedFlatActivities(GetOptions options = null);
        Task<StreamResponse<EnrichedNotificationActivity>> GetEnrichedNotificationActivities(GetOptions options = null);
        Task RemoveActivity(string activityId, bool foreignId = false);
        Task UnfollowFeed(IStreamFeed feedToUnfollow, bool keepHistory = false);
        Task UnfollowFeed(string targetFeedSlug, string targetUserId, bool keepHistory = false);
        Task UpdateActivities(IEnumerable<Activity> activities);
        Task UpdateActivity(Activity activity);
    }
}
