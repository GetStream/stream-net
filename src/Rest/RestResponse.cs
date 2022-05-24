using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream.Rest
{
    internal class RestResponse
    {
        internal HttpStatusCode StatusCode { get; set; }

        internal string Content { get; set; }

        internal static async Task<RestResponse> FromResponseMessage(HttpResponseMessage message)
        {
            var response = new RestResponse { StatusCode = message.StatusCode };

            using (message)
            {
                response.Content = await message.Content.ReadAsStringAsync();
            }

            return response;
        }
    }
}
