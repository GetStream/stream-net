using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>
    /// Client to interact with batch operations.
    /// </summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
    public interface IBatchOperations
    {
        /// <summary>Add an activity to multiple feeds.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<ResponseBase> AddToManyAsync(Activity activity, IEnumerable<IStreamFeed> feeds);

        /// <summary>Add an activity to multiple feeds.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<ResponseBase> AddToManyAsync(Activity activity, IEnumerable<string> feedIds);

        /// <summary>Follow muiltiple feeds.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<ResponseBase> FollowManyAsync(IEnumerable<Follow> follows, int activityCopyLimit = 300);

        /// <summary>Get multiple activities by activity ids.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<GenericGetResponse<Activity>> GetActivitiesByIdAsync(IEnumerable<string> ids);

        /// <summary>Get multiple activities by foreign ids.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<GenericGetResponse<Activity>> GetActivitiesByForeignIdAsync(IEnumerable<ForeignIdTime> ids);

        /// <summary>Get multiple enriched activities.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<GenericGetResponse<EnrichedActivity>> GetEnrichedActivitiesAsync(IEnumerable<string> ids, GetOptions options = null);

        /// <summary>Get multiple enriched activities.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<GenericGetResponse<EnrichedActivity>> GetEnrichedActivitiesAsync(IEnumerable<ForeignIdTime> foreignIdTimes, GetOptions options = null);

        /// <summary>Updates multiple activities.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<ResponseBase> UpdateActivitiesAsync(IEnumerable<Activity> activities);

        /// <summary>Update a single activity.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<ResponseBase> UpdateActivity(Activity activity);

        /// <summary>Update multiple activities partially.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/add_many_activities/?language=csharp</remarks>
        Task<ResponseBase> ActivitiesPartialUpdateAsync(IEnumerable<ActivityPartialUpdateRequestObject> updates);
    }
}
