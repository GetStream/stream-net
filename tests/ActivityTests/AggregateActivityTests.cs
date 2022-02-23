using NUnit.Framework;
using Stream.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class AggregateActivityTests : TestBase
    {
        [Test]
        public async Task TestAggregate()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            var response = await UserFeed.AddActivityAsync(newActivity1);
            response = await UserFeed.AddActivityAsync(newActivity2);

            await AggregateFeed.FollowFeedAsync(this.UserFeed);

            var activities = (await this.AggregateFeed.GetAggregateActivitiesAsync()).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(2, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await AggregateFeed.UnfollowFeedAsync(this.UserFeed);
        }

        [Test]
        public async Task TestMixedAggregate()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            var newActivity3 = new Activity("1", "other", "2");
            var response = await UserFeed.AddActivityAsync(newActivity1);
            response = await UserFeed.AddActivityAsync(newActivity2);
            response = await UserFeed.AddActivityAsync(newActivity3);

            await AggregateFeed.FollowFeedAsync(this.UserFeed);

            var activities = (await this.AggregateFeed.GetAggregateActivitiesAsync(null)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(1, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await AggregateFeed.UnfollowFeedAsync(this.UserFeed);
        }

        [Test]
        public async Task TestGetAggregate()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            await UserFeed.AddActivityAsync(newActivity1);
            await UserFeed.AddActivityAsync(newActivity2);

            await AggregateFeed.FollowFeedAsync(this.UserFeed);

            var result = await this.AggregateFeed.GetAggregateActivitiesAsync();
            var activities = result.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var aggActivity = activities.First();
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(2, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);
            Assert.IsNotNull(aggActivity.CreatedAt);
            Assert.IsTrue(Math.Abs(aggActivity.CreatedAt.Value.Subtract(DateTime.UtcNow).TotalMinutes) < 10);
            Assert.IsNotNull(aggActivity.UpdatedAt);
            Assert.IsTrue(Math.Abs(aggActivity.UpdatedAt.Value.Subtract(DateTime.UtcNow).TotalMinutes) < 10);
            Assert.IsNotNull(aggActivity.Group);

            await AggregateFeed.UnfollowFeedAsync(this.UserFeed);
        }
    }
}