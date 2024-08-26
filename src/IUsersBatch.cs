using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    public interface IUsersBatch
    {
        Task<IEnumerable<User>> UpsertUsersAsync(IEnumerable<User> users, bool overrideExisting = false);
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds);
        Task<IEnumerable<string>> DeleteUsersAsync(IEnumerable<string> userIds);
    }
}