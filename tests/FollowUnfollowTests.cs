using NUnit.Framework;
using Stream.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class FollowUnfollowTests : TestBase
    {
        [Test]
        public async Task TestFlatFollowActivityCopyLimitNonDefault()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivities = new[]
                {
                    new Activity("1", "test", "1"),
                    new Activity("1", "test", "2"),
                    new Activity("1", "test", "3"),
                    new Activity("1", "test", "4"),
                    new Activity("1", "test", "5"),
                };
                var response = await this.FlatFeed.AddActivitiesAsync(newActivities);

                await this.UserFeed.FollowFeedAsync(this.FlatFeed, 3);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 5)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(3, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowByFeedKeepHistoryDefault()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this.FlatFeed.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this.UserFeed.UnfollowFeedAsync(FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowByFeedKeepHistoryFalse()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this.FlatFeed.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this.UserFeed.UnfollowFeedAsync(FlatFeed, false);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowActivityCopyLimitDefault()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivities = new[]
                {
                    new Activity("1", "test", "1"),
                    new Activity("1", "test", "2"),
                    new Activity("1", "test", "3"),
                    new Activity("1", "test", "4"),
                    new Activity("1", "test", "5"),
                };
                var response = await this.FlatFeed.AddActivitiesAsync(newActivities);

                await this.UserFeed.FollowFeedAsync(this.FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 5)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(5, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowByFeedKeepHistoryTrue()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this.FlatFeed.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this.UserFeed.UnfollowFeedAsync(FlatFeed, true);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowByFeedActivityCopyLimitDefault()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await this.FlatFeed.AddActivitiesAsync(newActivities);

                await this.UserFeed.FollowFeedAsync(FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 5)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(5, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowByFeedActivityCopyLimitNonDefault()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivities = new[]
                {
                    new Activity("1", "test", "1"),
                    new Activity("1", "test", "2"),
                    new Activity("1", "test", "3"),
                    new Activity("1", "test", "4"),
                    new Activity("1", "test", "5"),
                };
                var response = await this.FlatFeed.AddActivitiesAsync(newActivities);

                await this.UserFeed.FollowFeedAsync(FlatFeed, 3);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 5)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(3, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryDefault()
        {
            var secret = Client.Feed("secret", System.Guid.NewGuid().ToString());

            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(secret);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await secret.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(secret);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this.UserFeed.UnfollowFeedAsync(secret);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryFalse()
        {
            var secret = Client.Feed("secret", System.Guid.NewGuid().ToString());

            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(secret);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await secret.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(secret);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this.UserFeed.UnfollowFeedAsync(secret, false);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryTrue()
        {
            var secret = Client.Feed("secret", System.Guid.NewGuid().ToString());

            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(secret);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await secret.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(secret);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this.UserFeed.UnfollowFeedAsync(secret, true);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowPrivateActivityCopyLimitDefault()
        {
            var secret = Client.Feed("secret", System.Guid.NewGuid().ToString());

            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(secret, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await secret.AddActivitiesAsync(newActivities);

                await this.UserFeed.FollowFeedAsync(secret);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 5)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(5, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowPrivateActivityCopyLimitNonDefault()
        {
            var secret = Client.Feed("secret", System.Guid.NewGuid().ToString());

            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(secret, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await secret.AddActivitiesAsync(newActivities);

                await this.UserFeed.FollowFeedAsync(secret, 3);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 5)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(3, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryTrue()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            var newActivity = new Activity("1", "test", "1");
            var response = await this.FlatFeed.AddActivityAsync(newActivity);

            await this.UserFeed.FollowFeedAsync(this.FlatFeed);

            activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);

            // Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
            await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, true);

            activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
        }

        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryDefault()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this.FlatFeed.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(this.FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this.UserFeed.UnfollowFeedAsync(this.FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryFalse()
        {
            // This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, false);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this.FlatFeed.AddActivityAsync(newActivity);

                await this.UserFeed.FollowFeedAsync(this.FlatFeed);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                // Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this.UserFeed.UnfollowFeedAsync(this.FlatFeed, false);

                activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFollowingsWithLimit()
        {
            var feed = Client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed1 = Client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed2 = Client.Feed("flat", System.Guid.NewGuid().ToString());

            // unfollow
            await feed1.UnfollowFeedAsync(feed);
            await feed1.UnfollowFeedAsync(feed2);

            var response = (await feed1.FollowingAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeedAsync(feed);
            await feed1.FollowFeedAsync(feed2);

            response = (await feed1.FollowingAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());
            Assert.AreEqual(response.First().FeedId, feed1.FeedId);
            Assert.AreEqual(response.First().TargetId, feed2.FeedId);
        }

        [Test]
        public async Task TestDoIFollowEmpty()
        {
            var lonely = Client.Feed("flat", "lonely");
            var response = (await lonely.FollowingAsync(0, 10, new[] { "flat:asocial" })).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowersEmpty()
        {
            var lonely = Client.Feed("flat", "lonely");
            var response = (await lonely.FollowersAsync()).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowersWithLimit()
        {
            var feed = Client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed1 = Client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed2 = Client.Feed("flat", System.Guid.NewGuid().ToString());

            // unfollow
            await feed1.UnfollowFeedAsync(feed);
            await feed2.UnfollowFeedAsync(feed);

            var response = (await feed.FollowersAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeedAsync(feed);
            await feed2.FollowFeedAsync(feed);

            response = (await feed.FollowersAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var first = response.First();
            Assert.AreEqual(first.FeedId, feed2.FeedId);
            Assert.AreEqual(first.TargetId, feed.FeedId);
            Assert.IsTrue(first.CreatedAt > DateTime.Now.AddDays(-1));
        }

        [Test]
        public async Task TestFollowingEmpty()
        {
            var lonely = Client.Feed("flat", "lonely");
            var response = (await lonely.FollowingAsync()).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowStats()
        {
            var f1 = Client.Feed("user", System.Guid.NewGuid().ToString());
            var f2 = Client.Feed("user", System.Guid.NewGuid().ToString());
            await f1.FollowFeedAsync(f2);

            var stats = (await f1.FollowStatsAsync(null, new[] { "timeline" })).Results;
            Assert.AreEqual(stats.Followers.Count, 0);
            Assert.AreEqual(stats.Followers.Feed, f1.FeedId);
            Assert.AreEqual(stats.Following.Count, 0);
            Assert.AreEqual(stats.Following.Feed, f1.FeedId);

            stats = (await f1.FollowStatsAsync(null, new[] { "user" })).Results;
            Assert.AreEqual(stats.Followers.Count, 0);
            Assert.AreEqual(stats.Followers.Feed, f1.FeedId);
            Assert.AreEqual(stats.Following.Count, 1);
            Assert.AreEqual(stats.Following.Feed, f1.FeedId);
        }
    }
}