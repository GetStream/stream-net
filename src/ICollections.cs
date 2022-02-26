using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>
    /// <para>Client to interact with collections.</para>
    /// Collections enable you to store information to Stream. This allows you to use it inside your feeds,
    /// and to provide additional data for the personalized endpoints. Examples include products and articles,
    /// but any unstructured object (e.g. JSON) is a good match for collections.
    /// </summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
    public interface ICollections
    {
        /// <summary>Creates a new collection.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<CollectionObject> AddAsync(string collectionName, Dictionary<string, object> data, string id = null, string userId = null);

        /// <summary>Deletes a collection.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<ResponseBase> DeleteAsync(string collectionName, string id);

        /// <summary>Deletes multiple collections.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task DeleteManyAsync(string collectionName, IEnumerable<string> ids);

        /// <summary>Returns a collection by id.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<CollectionObject> GetAsync(string collectionName, string id);

        /// <summary>Returns a collection object.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<GetCollectionResponseObject> SelectAsync(string collectionName, string id);

        /// <summary>Returns multiple collection objects.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<GetCollectionResponseWrap> SelectManyAsync(string collectionName, IEnumerable<string> ids);

        /// <summary>Updates a specific collection object.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<CollectionObject> UpdateAsync(string collectionName, string id, Dictionary<string, object> data);

        /// <summary>Creates or updates a collection.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<ResponseBase> UpsertAsync(string collectionName, CollectionObject data);

        /// <summary>Creates or updates multiple collections.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/collections_introduction/?language=csharp</remarks>
        Task<ResponseBase> UpsertManyAsync(string collectionName, IEnumerable<CollectionObject> data);

        /// <summary>Returns a reference identifier to the collection object.</summary>
        string Ref(string collectionName, string collectionObjectId);

        /// <summary>Returns a reference identifier to the collection object.</summary>
        string Ref(string collectionName, CollectionObject obj);
    }
}
