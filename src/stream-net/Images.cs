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

    public async Task<Image> Upload(System.IO.Stream image, string contentType)
    {
      var request = _client.BuildUploadRequest(this);
      
      request.SetFileStream(image, contentType);

      var response = await _client.MakeRequest(request);

      if (response.StatusCode == System.Net.HttpStatusCode.Created)
        return JsonConvert.DeserializeObject<Image>(response.Content);

      throw StreamException.FromResponse(response);
    }

  }
}