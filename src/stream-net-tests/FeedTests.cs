using NUnit.Framework;
using System;

namespace StreamNetTests
{
    [Parallelizable(ParallelScope.Self)]
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
        public void TestAddActivityArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.AddActivity(null);
            });
        }

        [Test]
        public void TestAddActivitiesArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.AddActivities(null);
            });
        }

        [Test]
        public void TestUpdateActivityArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.UpdateActivity(null);
            });
        }

        [Test]
        public void TestUpdateActivitiesArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.UpdateActivities(null);
            });
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.UpdateActivities(new Stream.Activity[101]);
            });
        }

        [Test]
        public void TestGetActivitiesArguments()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _feed.GetActivities(-1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _feed.GetActivities(0, -2);
            });
        }

        [Test]
        public void TestFollowFeedArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.FollowFeed(null);
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _feed.FollowFeed(_feed);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeed(feed, -1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeed(feed, 1001);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeed(feed, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                var feed = _client.Feed("flat", Guid.NewGuid().ToString());
                await _feed.FollowFeed(feed, 1000);
            });
        }

        [Test]
        public void TestUnfollowFeedArguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _feed.UnfollowFeed(null);
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _feed.UnfollowFeed(_feed);
            });
        }

        [Test]
        public void TestFollowersArguments()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _feed.Followers(-1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _feed.Followers(0, -2);
            });
        }

        [Test]
        public void TestFollowingArguments()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _feed.Following(-1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _feed.Following(0, -2);
            });
        }
    }
}
