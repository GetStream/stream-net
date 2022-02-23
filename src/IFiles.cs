using Stream.Models;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>Client to interact with files.</summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/files_introduction/?language=csharp</remarks>
    public interface IFiles
    {
        /// <summary>Delete a file using it's URL.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/files_introduction/?language=csharp</remarks>
        Task DeleteAsync(string url);

        /// <summary>Upload a file.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/files_introduction/?language=csharp</remarks>
        Task<Upload> UploadAsync(System.IO.Stream file, string name, string contentType = null);
    }
}
