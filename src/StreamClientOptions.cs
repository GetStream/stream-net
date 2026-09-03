using System.Collections.Generic;
using System.Net.Http;

namespace Stream
{
    /// <summary>Customization options for the internal HTTP client.</summary>
    public class StreamClientOptions
    {
        /// <summary>Default settings where the backend location is <see cref="StreamApiLocation.USEast"/>.</summary>
        public static StreamClientOptions Default => new StreamClientOptions();

        /// <summary>
        /// Number of milliseconds to wait on requests
        /// </summary>
        /// <remarks>Default is 3000</remarks>
        public int Timeout { get; set; } = 3000;

        /// <summary>
        /// Number of milliseconds to wait on requests to personalization
        /// </summary>
        /// <remarks>Default is 3000</remarks>
        public int PersonalizationTimeout { get; set; } = 3000;

        /// <summary>
        /// Backend location of Stream API.
        /// </summary>
        /// <remarks>Default is US East</remarks>
        public StreamApiLocation Location { get; set; } = StreamApiLocation.USEast;

        /// <summary>
        /// Personalization backend location.
        /// </summary>
        /// <remarks>Default is US East.</remarks>
        public StreamApiLocation PersonalizationLocation { get; set; } = StreamApiLocation.USEast;

        /// <summary>
        /// Middleware that wraps every HTTP request the client makes, letting you observe or
        /// decorate requests and responses. The first handler in the list is the outermost one:
        /// it sees the request first and the response last.
        /// </summary>
        /// <remarks>
        /// A handler receives the raw <see cref="HttpResponseMessage"/>, so it can read response
        /// headers the SDK does not surface itself, such as the rate limit headers described at
        /// https://getstream.io/docs/platform/rate-limits/.
        /// <para>Handler instances are bound to the client they are passed to, so give every
        /// <see cref="StreamClient"/> its own handler instances.</para>
        /// </remarks>
        public IList<DelegatingHandler> HttpHandlers { get; } = new List<DelegatingHandler>();
    }

    /// <summary>Physical location of the backend.</summary>
    public enum StreamApiLocation
    {
        /// <summary>United States, east coast</summary>
        USEast,

        /// <summary>Dublin</summary>
        Dublin,

        /// <summary>Tokyo</summary>
        Tokyo,

        /// <summary>Mumbai</summary>
        Mumbai,

        /// <summary>Singapore</summary>
        Singapore,

        /// <summary>Sidney</summary>
        Sidney,

        /// <summary>Oregon</summary>
        Oregon,

        /// <summary>Ohio</summary>
        Ohio,
    }
}
