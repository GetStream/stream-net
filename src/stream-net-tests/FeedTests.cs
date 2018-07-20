using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class FeedTests
    {
        private Stream.StreamClient _client;
        private Stream.StreamFeed _feed;

        [SetUp]
        public void Setup()
        {
            _client = new Stream.StreamClient(
                "98a6bhskrrwj",
                "t3nj7j8m6dtdbbakzbu9p7akjk5da8an5wxwyt6g73nt5hf9yujp8h4jw244r67p",
                new Stream.StreamClientOptions()
                {
                    Location = Stream.StreamApiLocation.USEast
                });
            _feed = _client.Feed("flat", "42");
        }

        [Test]
        public void TestAddActivityArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.AddActivity(null).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestAddActivitiesArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.AddActivities(null).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestUpdateActivityArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.UpdateActivity(null).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestUpdateActivitiesArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.UpdateActivities(null).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.UpdateActivities(new Stream.Activity[101]).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestGetActivitiesArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _feed.GetActivities(-1).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _feed.GetActivities(0, -2).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestFollowFeedArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.FollowFeed(null).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentException>(() =>
            {
                _feed.FollowFeed(_feed).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestUnfollowFeedArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.UnfollowFeed(null).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentException>(() =>
            {
                _feed.UnfollowFeed(_feed).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestFollowersArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _feed.Followers(-1).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _feed.Followers(0, -2).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestFollowingArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _feed.Following(-1).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _feed.Following(0, -2).GetAwaiter().GetResult();
            });
        }
    }
}
