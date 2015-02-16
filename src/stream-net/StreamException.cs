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
            public int Code { get; set; }
            public String Detail { get; set; }
            public String Exception { get; set; }

            [Newtonsoft.Json.JsonProperty("status_code")]
            public int HttpStatusCode { get; set; }
        }

        public static StreamException FromResponse(IRestResponse response)
        {
            //{"code": 6, "detail": "The following feeds are not configured: 'secret'", "duration": "4ms", "exception": "FeedConfigException", "status_code": 400}

            var state = JsonConvert.DeserializeObject<ExceptionState>(response.Content);
            
            throw new StreamException(state);
        }
    }
}
