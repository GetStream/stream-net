using Stream.Models;
using Stream.Utils;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class Images : IImages
    {
        private readonly StreamClient _client;

        internal Images(StreamClient client)
        {
            _client = client;
        }

        public async Task<Upload> UploadAsync(System.IO.Stream image, string name, string contentType)
        {
            var request = _client.BuildImageUploadRequest();

            request.SetFileStream(image, name, contentType);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<Upload>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task DeleteAsync(string url)
        {
            var request = _client.BuildAppRequest("images/", HttpMethod.Delete);
            request.AddQueryParameter("url", url);

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }
    }
}
