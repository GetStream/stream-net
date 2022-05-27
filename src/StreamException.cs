using Newtonsoft.Json;
using Stream.Models;
using Stream.Rest;
using Stream.Utils;
using System;
using System.Net;

namespace Stream
{
    internal class ExceptionResponse : ResponseBase
    {
        public int Code { get; set; }
        public string Detail { get; set; }
        public string Exception { get; set; }
        public string MoreInfo { get; set; }

        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; set; }
    }

    /// <summary>
    /// <para>Generic exception that was thrown by the backend.</para>
    /// For more details check the <see cref="Detail"/> property. The error code
    /// that can be found in the <see cref="ErrorCode"/> property can be check in
    /// the below webpage.
    /// </summary>
    /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/api_error_responses/?language=csharp</remarks>
#if !NETCORE
    [Serializable]
#endif
    public class StreamException : Exception
    {
        internal StreamException(ExceptionResponse state) : base($"{state.Exception}: {state.Detail}l")
        {
            ErrorCode = state.Code;
            HttpStatusCode = state.HttpStatusCode;
            Detail = state.Detail;
            MoreInfo = state.MoreInfo;
        }

        /// <summary>Error code from the backend.</summary>
        /// <remarks>https://getstream.io/activity-feeds/docs/dotnet-csharp/api_error_responses/?language=csharp</remarks>
        public int ErrorCode { get; }

        /// <summary>HTTP code returned from the backend.</summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>Details of the error.</summary>
        public string Detail { get; }

        /// <summary>An URL that provides more information about the error.</summary>
        public string MoreInfo { get; }

        internal static StreamException FromResponse(RestResponse response)
        {
            // If we get an error response from getstream.io with the following structure then use it to populate the exception details,
            // otherwise fill in the properties from the response, the most likely case being when we get a timeout.
            // {"code": 6, "detail": "The following feeds are not configured: 'secret'", "duration": "4ms", "exception": "FeedConfigException", "status_code": 400}
            ExceptionResponse state = null;

            if (!string.IsNullOrWhiteSpace(response.Content) && response.Content.TrimStart().StartsWith("{"))
            {
                state = StreamJsonConverter.DeserializeObject<ExceptionResponse>(response.Content);
            }

            state = state ?? new ExceptionResponse();
            state.HttpStatusCode = response.StatusCode;

            throw new StreamException(state);
        }
    }
}
