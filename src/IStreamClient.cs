using Stream.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>Base client for the Stream API.</summary>
    public interface IStreamClient
    {
        /// <summary>
        /// Returns an <see cref="IBatchOperations"/> instance that let's you interact with batch operations.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        IBatchOperations Batch { get; }

        /// <summary>
        /// Returns an <see cref="ICollections"/> instance that let's you interact with collections.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        ICollections Collections { get; }

        /// <summary>
        /// Returns an <see cref="IReactions"/> instance that let's you interact with reactions.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        IReactions Reactions { get; }

        /// <summary>
        /// Returns an <see cref="IUsers"/> instance that let's you interact with users.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        IUsers Users { get; }

        /// <summary>
        /// Returns an <see cref="IPersonalization"/> instance that let's you interact with personalizations.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        IPersonalization Personalization { get; }

        /// <summary>
        /// Returns an <see cref="IFiles"/> instance that let's you interact with files.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        IFiles Files { get; }

        /// <summary>
        /// Returns an <see cref="IImages"/> instance that let's you interact with images.
        /// You can used the returned instance as a singleton in your application.
        /// </summary>
        IImages Images { get; }

        /// <summary>
        /// Partial update of an <see cref="Activity"/>.
        /// </summary>
        Task<ResponseBase> ActivityPartialUpdateAsync(string id = null, ForeignIdTime foreignIdTime = null, Dictionary<string, object> set = null, IEnumerable<string> unset = null);

        /// <summary>
        /// Returns an <see cref="IStreamFeed"/> instance that let's you interact with feeds.
        /// </summary>
        IStreamFeed Feed(string feedSlug, string userId);

        /// <summary>
        /// Allows you to retrieve open graph information from a URL which you can then use to add images and a description to activities.
        /// </summary>
        Task<Og> OgAsync(string url);

        /// <summary>
        /// Creates a JWT for the given <paramref name="userId"/>.
        /// </summary>
        string CreateUserToken(string userId, IDictionary<string, object> extraData = null);
    }
}
