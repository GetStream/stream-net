using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>
    /// <para>Client to interact with users.</para>
    /// Stream allows you to store user information and embed them inside
    /// activities or use them for personalization. When stored in activities,
    /// users are automatically enriched by Stream.
    /// </summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
    public interface IUsers
    {
        /// <summary>Creates a new user.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/users_introduction</remarks>
        Task<User> AddAsync(string userId, IDictionary<string, object> data = null, bool getOrCreate = false);

        /// <summary>Deletes a user.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/users_introduction</remarks>
        Task DeleteAsync(string userId);

        /// <summary>Retrieves a user.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/users_introduction</remarks>
        Task<User> GetAsync(string userId);

        /// <summary>Updates a user.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/users_introduction</remarks>
        Task<User> UpdateAsync(string userId, IDictionary<string, object> data);

        /// <summary>Returns a reference identifier to the user id.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/users_introduction</remarks>
        string Ref(string userId);

        /// <summary>Returns a reference identifier to the user object.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/users_introduction</remarks>
        string Ref(User obj);
    }
}
