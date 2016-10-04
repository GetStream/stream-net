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
        public void TestAddActivityArugments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.AddActivity(null).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestAddActivitiesArugments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.AddActivities(null).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestUpdateActivityArugments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _feed.UpdateActivity(null).GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestUpdateActivitiesArugments()
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
        public void TestGetActivitiesArugments()
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
        public void TestFollowFeedArugments()
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
        public void TestUnfollowFeedArugments()
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
        public void TestFollowersArugments()
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
        public void TestFollowingArugments()
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
