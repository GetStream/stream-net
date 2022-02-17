using Stream.Models;
using Stream.Utils;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class Files : IFiles
    {
        private readonly StreamClient _client;

        public Files(StreamClient client)
        {
            _client = client;
        }

        public async Task<Upload> UploadAsync(System.IO.Stream file, string name, string contentType = null)
        {
            var request = _client.BuildFileUploadRequest();
            request.SetFileStream(file, name, contentType);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<Upload>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task DeleteAsync(string url)
        {
            var request = _client.BuildAppRequest("files/", HttpMethod.Delete);
            request.AddQueryParameter("url", url);

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }
    }
}
