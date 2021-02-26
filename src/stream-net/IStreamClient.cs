using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    public interface IStreamClient
    {
        IBatchOperations Batch { get; }
        Collections Collections { get; }
        Reactions Reactions { get; }
        Users Users { get; }
        Personalization Personalization { get; }

        Files Files { get; }
        Images Images { get; }

        Task ActivityPartialUpdate(string id = null, ForeignIDTime foreignIDTime = null, GenericData set = null, IEnumerable<string> unset = null);
        IStreamFeed Feed(string feedSlug, string userId);
        Task<Og> Og(string url);
        string CreateUserToken(string userId, IDictionary<string, object> extraData = null);
    }
}
