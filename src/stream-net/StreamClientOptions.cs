using System;

namespace Stream
{
    public enum StreamApiLocation
    {
        USEast,
        Dublin,
        Singapore,
        Tokyo,

        [Obsolete("This api location is no longer in use and will be removed in the future")]
        USWest,
        [Obsolete("This api location is no longer in use and will be removed in the future")]
        EUCentral,
    }

    public class StreamClientOptions
    {
        public static StreamClientOptions Default = new StreamClientOptions();

        /// <summary>
        /// Number of milliseconds to wait on requests
        /// </summary>
        /// <remarks>Default is 3000</remarks>
        public int Timeout { get; set; }

        /// <summary>
        /// Number of milliseconds to wait on requests to personalization
        /// </summary>
        /// <remarks>Default is 3000</remarks>
        public int PersonalizationTimeout { get; set; }

        public StreamApiLocation Location { get; set; }

        public StreamApiLocation PersonalizationLocation { get; set; }

        public bool ExpireTokens { get; set; }

        public StreamClientOptions()
        {
            ExpireTokens = false;
            Location = StreamApiLocation.USEast;
            Timeout = 3000;
            PersonalizationTimeout = 3000;
            PersonalizationLocation = StreamApiLocation.USEast;
        }
    }
}
