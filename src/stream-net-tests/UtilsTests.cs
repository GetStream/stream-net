using System;

namespace stream_net_tests
{
    public class Credentials
    {
        public static Credentials Instance = new Credentials();

        public Stream.StreamClient Client
        {
            get
            {
                return _client;
            }
        }

        private readonly Stream.StreamClient _client;

        internal Credentials()
        {
            _client = new Stream.StreamClient(
                Environment.GetEnvironmentVariable("STREAM_API_KEY"),
                Environment.GetEnvironmentVariable("STREAM_API_SECRET"),
                new Stream.StreamClientOptions()
                {
                    Location = Stream.StreamApiLocation.USEast,
                    Timeout = 10000
                });
        }
    }
}
