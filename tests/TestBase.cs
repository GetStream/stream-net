using NUnit.Framework;
using Stream;

namespace StreamNetTests
{
    public abstract class TestBase
    {
        protected IStreamClient Client { get; private set; }
        protected IStreamFeed UserFeed { get; private set; }
        protected IStreamFeed UserFeed2 { get; private set; }
        protected IStreamFeed RankedFeed { get; private set; }
        protected IStreamFeed FlatFeed { get; private set; }
        protected IStreamFeed AggregateFeed { get; private set; }
        protected IStreamFeed NotificationFeed { get; private set; }

        [SetUp]
        public void Setup()
        {
            Client = Credentials.Instance.Client;
            UserFeed = Client.Feed("user", System.Guid.NewGuid().ToString());
            UserFeed2 = Client.Feed("user", System.Guid.NewGuid().ToString());
            FlatFeed = Client.Feed("flat", System.Guid.NewGuid().ToString());
            AggregateFeed = Client.Feed("aggregate", System.Guid.NewGuid().ToString());
            NotificationFeed = Client.Feed("notification", System.Guid.NewGuid().ToString());
            RankedFeed = Client.Feed("ranked", System.Guid.NewGuid().ToString());
        }
    }
}