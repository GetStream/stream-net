using NUnit.Framework;
using System;

namespace StreamNetTests
{
    [TestFixture]
    public class FeedTests
    {
        private Stream.IStreamClient _client;
        private Stream.IStreamFeed _feed;

        [SetUp]
        public void Setup()
        {
            _client = Credentials.Instance.Client;
            _feed = _client.Feed("flat", "42");
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
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, -1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, 1001);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeedAsync(feed, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
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
