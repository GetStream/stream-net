
namespace Stream
{
    public enum StreamApiLocation
    {
        USEast,
        USWest,
        EUWest,
        AsiaJapan
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

        public StreamClientOptions()
        {
            Timeout = 3000;
            Location = StreamApiLocation.USEast;
        }
    }
}
