using Newtonsoft.Json;
using RestSharp;
using System;

namespace Stream
{
    [Serializable]
    public class StreamException : Exception
    {
        internal StreamException(ExceptionState state)
            : base(message: state.Detail)
        {
        
        }

        internal class ExceptionState
        {
            public int? Code { get; set; }

            public string Detail { get; set; }

            public string Exception { get; set; }

            [Newtonsoft.Json.JsonProperty("status_code")]
            public int HttpStatusCode { get; set; }
        }

        internal static StreamException FromResponse(IRestResponse response)
        {
            //If we get an error response from getstream.io with the following structure then use it to populate the exception details, 
            //otherwise fill in the properties from the response, the most likely case being when we get a timeout.
            //{"code": 6, "detail": "The following feeds are not configured: 'secret'", "duration": "4ms", "exception": "FeedConfigException", "status_code": 400}

            ExceptionState state = null;
            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                state = JsonConvert.DeserializeObject<ExceptionState>(response.Content);
            }
            if (state == null)
            {
                state = new ExceptionState() { Code = null, Detail = response.ErrorMessage, @Exception = response.ErrorException.ToString(), HttpStatusCode = (int)response.StatusCode };
            }
            throw new StreamException(state);
        }
    }
}
