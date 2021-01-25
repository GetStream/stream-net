﻿using NUnit.Framework;
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
