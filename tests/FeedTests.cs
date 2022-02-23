using NUnit.Framework;
using Stream;
using System;

namespace StreamNetTests
{
    [TestFixture]
    public class FeedTests : TestBase
    {
        private IStreamFeed _feed;

        [SetUp]
        public void SetupFeed()
        {
            _feed = Client.Feed("flat", "42");
        }

        [Test]
        public void TestFollowFeedArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.FollowFeedAsync(null);
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _feed.FollowFeedAsync(_feed);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var feed = Client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, -1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var feed = Client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, 1001);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                var feed = Client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                var feed = Client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, 1000);
            });
        }

        [Test]
        public void TestUnfollowFeedArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.UnfollowFeedAsync(null);
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _feed.UnfollowFeedAsync(_feed);
            });
        }
    }
}
