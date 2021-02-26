using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Stream
{
    public class Images
    {
        readonly StreamClient _client;

        public Images(StreamClient client)
        {
            _client = client;
        }

        public async Task<Upload> Upload(System.IO.Stream image, string name, string contentType)
        {
            var request = _client.BuildImageUploadRequest();

            request.SetFileStream(image, name, contentType);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return JsonConvert.DeserializeObject<Upload>(response.Content);

            throw StreamException.FromResponse(response);
        }
    }

    public class Files
    {
        readonly StreamClient _client;

        public Files(StreamClient client)
        {
            _client = client;
        }

        public async Task<Upload> Upload(System.IO.Stream file, string name, string contentType = null)
        {
            var request = _client.BuildFileUploadRequest();
            request.SetFileStream(file, name, contentType);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return JsonConvert.DeserializeObject<Upload>(response.Content);

            throw StreamException.FromResponse(response);
        }
    }
}
