using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    public interface IModeration
    {
        Task<ResponseBase> FlagUserAsync(string flaggedUserId, string reason, IDictionary<string, object> options = null);

        Task<ResponseBase> FlagActivityAsync(string entityId, string entityCreatorId, string reason,
            IDictionary<string, object> options = null);

        Task<ResponseBase> FlagReactionAsync(string entityId, string entityCreatorId, string reason,
            IDictionary<string, object> options = null);
        Task<ResponseBase> FlagAsync(string entityType, string entityId, string entityCreatorId, string reason, IDictionary<string, object> options = null);
    }
}