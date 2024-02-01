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
