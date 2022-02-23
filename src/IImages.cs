using Stream.Models;
using System.Threading.Tasks;

namespace Stream
{
    /// <summary>Client to interact with images.</summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/files_introduction/?language=csharp</remarks>
    public interface IImages
    {
        /// <summary>Delete an image using it's URL.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/files_introduction/?language=csharp</remarks>
        Task DeleteAsync(string url);

        /// <summary>Upload an image.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/files_introduction/?language=csharp</remarks>
        Task<Upload> UploadAsync(System.IO.Stream image, string name, string contentType);
    }
}
