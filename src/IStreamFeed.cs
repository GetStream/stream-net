using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>
    /// <para>Client to interact with a specific feed.</para>
    /// A Feed is like a Stack (FILO) of activities. Activities can be pushed directly to a Feed. They can
    /// also be propagated from feeds that they follow (see: “Follow Relationships” and “Fan-out”).
    /// A single application may have multiple feeds. For example, you might have a user's feed (what they posted),
    /// their timeline feed (what the people they follow posted), and a notification feed (to alert them of
    /// engagement with activities they posted).
    /// </summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
    public interface IStreamFeed
    {
        /// <summary>The feed id is the unique identifier of the feed whichs consists of the feed slug and the user id.</summary>
        string FeedId { get; }

        /// <summary>The start of the relative url of the feed.</summary>
        string UrlPath { get; }

        /// <summary>The start of the relative url of the feed for enrichment.</summary>
        string EnrichedPath { get; }

        /// <summary>The start of the relative url of the feed for enrichment.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<AddActivitiesResponse> AddActivitiesAsync(IEnumerable<Activity> activities);

        /// <summary>Add a new activity to the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<Activity> AddActivityAsync(Activity activity);

        /// <summary>Returns a paginated list of the feed's followers.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<Follower>> FollowersAsync(int offset = 0, int limit = 25, IEnumerable<string> filterBy = null);

        /// <summary>Starts to follow another feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> FollowFeedAsync(IStreamFeed feedToFollow, int activityCopyLimit = 100);

        /// <summary>Starts to follow another feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> FollowFeedAsync(string targetFeedSlug, string targetUserId, int activityCopyLimit = 100);

        /// <summary>Returns the followings of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<Follower>> FollowingAsync(int offset = 0, int limit = 25, IEnumerable<string> filterBy = null);

        /// <summary>Returns activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<Activity>> GetActivitiesAsync(int offset = 0, int limit = 20, FeedFilter filter = null, ActivityMarker marker = null);

        /// <summary>Returns flat activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<Activity>> GetFlatActivitiesAsync(GetOptions options = null);

        /// <summary>Returns aggregate activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<AggregateActivity>> GetAggregateActivitiesAsync(GetOptions options = null);

        /// <summary>Returns notification activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<NotificationGetResponse> GetNotificationActivities(GetOptions options = null);

        /// <summary>Returns enriched flat activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<EnrichedActivity>> GetEnrichedFlatActivitiesAsync(GetOptions options = null);

        /// <summary>Returns enriched aggregate activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<EnrichedAggregateActivity>> GetEnrichedAggregatedActivitiesAsync(GetOptions options = null);

        /// <summary>Returns enriched notification activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<GenericGetResponse<EnrichedNotificationActivity>> GetEnrichedNotificationActivitiesAsync(GetOptions options = null);

        /// <summary>Removes an activity from the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> RemoveActivityAsync(string activityId, bool foreignId = false);

        /// <summary>Unfollows a feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> UnfollowFeedAsync(IStreamFeed feedToUnfollow, bool keepHistory = false);

        /// <summary>Unfollows a feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> UnfollowFeedAsync(string targetFeedSlug, string targetUserId, bool keepHistory = false);

        /// <summary>Updates activities of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> UpdateActivitiesAsync(IEnumerable<Activity> activities);

        /// <summary>Updates an activity of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<ResponseBase> UpdateActivityAsync(Activity activity);

        /// <summary>Returns following and follower stats of the feed.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<FollowStatsResponse> FollowStatsAsync(IEnumerable<string> followersSlugs = null, IEnumerable<string> followingSlugs = null);

        /// <summary>
        /// Updates the "to" targets for the provided activity, with the options passed
        /// as argument for replacing, adding, or removing to targets.
        /// </summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/feeds_101/?language=csharp</remarks>
        Task<UpdateToTargetsResponse> UpdateActivityToTargetsAsync(ForeignIdTime foreignIdTime,
            IEnumerable<string> adds = null,
            IEnumerable<string> newTargets = null,
            IEnumerable<string> removed = null);
    }
}
