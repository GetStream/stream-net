using Newtonsoft.Json;
using Stream.Rest;
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

        public async Task Delete(string url)
        {
            var request = _client.BuildAppRequest("images/", HttpMethod.DELETE);
            request.AddQueryParameter("url", url);

            var response = await _client.MakeRequest(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
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

        public async Task Delete(string url)
        {
            var request = _client.BuildAppRequest("files/", HttpMethod.DELETE);
            request.AddQueryParameter("url", url);

            var response = await _client.MakeRequest(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }
    }
}
