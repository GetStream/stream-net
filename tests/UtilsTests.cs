using Stream;
using System;

namespace StreamNetTests
{
    public class Credentials
    {
        internal Credentials()
        {
            Client = new StreamClient(
                Environment.GetEnvironmentVariable("STREAM_API_KEY"),
                Environment.GetEnvironmentVariable("STREAM_API_SECRET"),
                new StreamClientOptions
                {
                    Location = StreamApiLocation.USEast,
                    Timeout = 16000
                });
        }

        public static Credentials Instance = new Credentials();
        public StreamClient Client { get; }
    }
}
