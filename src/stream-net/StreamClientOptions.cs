
namespace Stream
{
    public enum StreamApiLocation
    {
        USEast,
        USWest,
        EUCentral
    }

    public class StreamClientOptions
    {
        public static StreamClientOptions Default = new StreamClientOptions();

        /// <summary>
        /// Number of milliseconds to wait on requests
        /// </summary>
        /// <remarks>Default is 3000</remarks>
        public int Timeout { get; set; }

        public StreamApiLocation Location { get; set; }

        public bool ExpireTokens { get; set; }

        public StreamClientOptions()
        {
            ExpireTokens = false;
            Location = StreamApiLocation.USEast;
            Timeout = 3000;
        }
    }
}
