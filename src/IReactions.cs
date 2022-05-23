using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>
    /// <para>Client to interact with reactions.</para>
    /// Reactions are a special kind of data that can be used to capture user interaction with specific activities.
    /// Common examples of reactions are likes, comments, and upvotes. Reactions are automatically returned to feeds'
    /// activities at read time when the reactions parameters are used.
    /// </summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
    public interface IReactions
    {
        /// <summary>Posts a new reaciton.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<Reaction> AddAsync(string reactionId, string kind, string activityId, string userId, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null);

        /// <summary>Posts a new reaciton.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<Reaction> AddAsync(string kind, string activityId, string userId, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null);

        /// <summary>Adds a new child reaction.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<Reaction> AddChildAsync(Reaction parent, string reactionId, string kind, string userId, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null);

        /// <summary>Adds a new child reaction.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<Reaction> AddChildAsync(Reaction parent, string kind, string userId, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null);

        /// <summary>Deletes a reactions.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task DeleteAsync(string reactionId);

        /// <summary>Retrieves reactions.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<IEnumerable<Reaction>> FilterAsync(ReactionFiltering filtering, ReactionPagination pagination);

        /// <summary>Retrieves reactions and its' activities.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<ReactionsWithActivity> FilterWithActivityAsync(ReactionFiltering filtering, ReactionPagination pagination);

        /// <summary>Retrieves a single reaction.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<Reaction> GetAsync(string reactionId);

        /// <summary>Updates a reaction.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/reactions_introduction/?language=csharp</remarks>
        Task<Reaction> UpdateAsync(string reactionId, IDictionary<string, object> data = null, IEnumerable<string> targetFeeds = null);
    }
}
