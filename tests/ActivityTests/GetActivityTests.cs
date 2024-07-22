using NUnit.Framework;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class GetActivityTests : TestBase
    {
        [Test]
        public async Task TestGet()
        {
            var newActivity = new Activity("1", "test", "1");
            var first = await this.UserFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "2");
            var second = await this.UserFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "3");
            var third = await this.UserFeed.AddActivityAsync(newActivity);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 2)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());
            Assert.AreEqual(third.Id, activities.First().Id);
            Assert.AreEqual(second.Id, activities.Skip(1).First().Id);

            activities = (await this.UserFeed.GetActivitiesAsync(1, 2)).Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            // $id_offset =  ['id_lt' => $third_id];
            activities = (await this.UserFeed.GetActivitiesAsync(0, 2, FeedFilter.Where().IdLessThan(third.Id))).Results;
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestGetFlatActivities()
        {
            var newActivity = new Activity("1", "test", "1");
            var first = await this.UserFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "2");
            var second = await this.UserFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "3");
            var third = await this.UserFeed.AddActivityAsync(newActivity);

            var response = await this.UserFeed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2));
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Duration);
            var activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());
            Assert.AreEqual(third.Id, activities.First().Id);
            Assert.AreEqual(second.Id, activities.Skip(1).First().Id);

            response = await this.UserFeed.GetFlatActivitiesAsync(GetOptions.Default.WithOffset(1).WithLimit(2));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            response = await this.UserFeed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2).WithFilter(FeedFilter.Where().IdLessThan(third.Id)));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            response = await this.UserFeed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2).WithSession("dummy").WithFilter(FeedFilter.Where().IdLessThan(third.Id)));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestGetActivitiesByID()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            var newActivity3 = new Activity("1", "other", "2");
            var addedActivities = new List<Activity>();

            var response = await this.UserFeed.AddActivityAsync(newActivity1);
            addedActivities.Add(response);
            response = await this.UserFeed2.AddActivityAsync(newActivity2);
            addedActivities.Add(response);
            response = await this.FlatFeed.AddActivityAsync(newActivity3);
            addedActivities.Add(response);

            var activities = (await Client.Batch.GetActivitiesByIdAsync(addedActivities.Select(a => a.Id))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(addedActivities.Count, activities.Count());

            activities.ForEach(a =>
            {
                var found = addedActivities.Find(x => x.Id == a.Id);
                Assert.NotNull(found);
                Assert.AreEqual(found.Actor, a.Actor);
                Assert.AreEqual(found.Object, a.Object);
                Assert.AreEqual(found.Verb, a.Verb);
            });
        }

        [Test]
        [Ignore("Test database has no ranked method at the moment")]
        public async Task TestRankingVars()
        {
            var newActivity1 = new Activity("1", "test", "1")
            {
                ForeignId = "r-test-1",
                Time = DateTime.Parse("2000-08-16T16:32:32"),
            };

            newActivity1.SetData("popularity", 123);

            var response = await this.UserFeed.AddActivityAsync(newActivity1);

            var newActivity2 = new Activity("1", "test", "2")
            {
                ForeignId = "r-test-2",
                Time = DateTime.Parse("2000-08-17T16:32:32"),
            };

            response = await this.UserFeed.AddActivityAsync(newActivity2);

            var ranking_vars = new Dictionary<string, object> { { "popularity", 666 } };
            var r2 = await this.UserFeed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2).WithRanking("popular").WithRankingVars(ranking_vars));
            Assert.NotNull(r2);
            Assert.AreEqual(2, r2.Results.Count);
            Assert.AreEqual(r2.Results[0].Score, 11.090528);
            Assert.AreEqual(r2.Results[1].Score, 0.99999917);
        }

        [Test]
        public async Task TestModerationTemplate()
        {
            var newActivity1 = new Activity("1", "test", "1")
            {
                ForeignId = "r-test-1",
                Time = DateTime.Parse("2000-08-16T16:32:32"),
            };

            newActivity1.SetData("popularity", 123);

            var response = await this.UserFeed.AddActivityAsync(newActivity1);

            var newActivity2 = new Activity("1", "test", "2")
            {
                ForeignId = "r-test-2",
                Time = DateTime.Parse("2000-08-17T16:32:32"),
            };

            response = await this.UserFeed.AddActivityAsync(newActivity2);

            var mod_template = "moderation_template_1";
            var r2 = await this.UserFeed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2).WithModerationTemplate(mod_template));
            Assert.NotNull(r2);
            Assert.AreEqual(2, r2.Results.Count);

            Assert.AreEqual(r2.Results[0].Moderation.Texts[0], "text");
            Assert.AreEqual(r2.Results[0].Moderation.Images[0], "image");
        }

        [Test]
        [Ignore("Test server doesn't support this feature at the moment")]
        public async Task TestActorFilter()
        {
            var feed = this.UserFeed;

            var newActivity1 = new Activity("1", "test", "1");
            var r1 = await feed.AddActivityAsync(newActivity1);

            var newActivity2 = new Activity("2", "test", "2");
            var r2 = await feed.AddActivityAsync(newActivity2);

            var r3 = await feed.GetFlatActivitiesAsync(GetOptions.Default.DiscardActors(new List<string> { "1" }, ";"));
            Assert.IsNotNull(r3);
            Assert.AreEqual(1, r3.Results.Count);

            var r4 = await feed.GetFlatActivitiesAsync(GetOptions.Default.DiscardActors(new List<string> { "1", "2" }, ";"));
            Assert.IsNotNull(r4);
            Assert.AreEqual(0, r4.Results.Count);
        }

        [Test]
        [Ignore("Test database has no ranked method at the moment")]
        public async Task TestScoreVars()
        {
            var feed = this.RankedFeed;

            var newActivity1 = new Activity("1", "test", "1")
            {
                ForeignId = "r-test-1",
                Time = DateTime.Parse("2000-08-16T16:32:32"),
            };

            newActivity1.SetData("popularity", 123);
            var r1 = await feed.AddActivityAsync(newActivity1);

            var r2 = await feed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(1).WithRanking("popularity").WithScoreVars());
            Assert.IsNotNull(r2.Results[0].ScoreVars);

            r2 = await feed.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(1).WithRanking("popularity"));
            Assert.IsNull(r2.Results[0].ScoreVars);
        }

        [Test]
        public async Task TestGetActivitiesByForeignIDAndTime()
        {
            var newActivity1 = new Activity("1", "test", "1")
            {
                ForeignId = "fid-test-1",
                Time = DateTime.Parse("2000-08-16T16:32:32"),
            };

            var newActivity2 = new Activity("1", "test", "2")
            {
                ForeignId = "fid-test-2",
                Time = DateTime.Parse("2000-08-17T16:32:32"),
            };

            var newActivity3 = new Activity("1", "other", "2")
            {
                ForeignId = "fid-other-1",
                Time = DateTime.Parse("2000-08-19T16:32:32"),
            };

            var addedActivities = new List<Activity>();

            var response = await this.UserFeed.AddActivityAsync(newActivity1);
            addedActivities.Add(response);
            response = await this.UserFeed2.AddActivityAsync(newActivity2);
            addedActivities.Add(response);
            response = await this.FlatFeed.AddActivityAsync(newActivity3);
            addedActivities.Add(response);

            var activities = (await Client.Batch.GetActivitiesByForeignIdAsync(
                addedActivities.Select(a => new ForeignIdTime(a.ForeignId, a.Time.Value)))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(addedActivities.Count, activities.Count());

            activities.ForEach(a =>
            {
                var found = addedActivities.Find(x => x.Id == a.Id);
                Assert.NotNull(found);
                Assert.AreEqual(found.Actor, a.Actor);
                Assert.AreEqual(found.Object, a.Object);
                Assert.AreEqual(found.Verb, a.Verb);
                Assert.AreEqual(found.ForeignId, a.ForeignId);
                Assert.AreEqual(found.Time, a.Time);
            });
        }
    }
}