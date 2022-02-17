using Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>Client to interract with personalization.</summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/personalization_introduction/?language=csharp</remarks>
    public interface IPersonalization
    {
        /// <summary>Removes data from the given resource, adding the given params to the request.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/personalization_introduction/?language=csharp</remarks>
        Task<ResponseBase> DeleteAsync(string endpoint, IDictionary<string, object> data);

        /// <summary>Returns data from the given resource, adding the given params to the request.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/personalization_introduction/?language=csharp</remarks>
        Task<IDictionary<string, object>> GetAsync(string endpoint, IDictionary<string, object> data);

        /// <summary>Sends data to the given resource, adding the given params to the request.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/personalization_introduction/?language=csharp</remarks>
        Task<IDictionary<string, object>> PostAsync(string endpoint, IDictionary<string, object> data);
    }
}
