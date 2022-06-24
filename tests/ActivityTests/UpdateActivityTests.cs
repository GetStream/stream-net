using NUnit.Framework;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class UpdateActivityTests : TestBase
    {
        [Test]
        public async Task TestUpdateActivities()
        {
            var newActivities = new[]
            {
                new Activity("multi1", "test", "1")
                {
                    ForeignId = "post:1",
                    Time = DateTime.UtcNow,
                },
                new Activity("multi2", "test", "2")
                {
                    ForeignId = "post:2",
                    Time = DateTime.UtcNow,
                },
            };

            var response = (await this.UserFeed.AddActivitiesAsync(newActivities)).Activities;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 2)).Results.ToArray();
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            Assert.AreEqual(response.First().Id, activities.First().Id);
            Assert.AreEqual(response.Skip(1).First().Id, activities.Skip(1).First().Id);

            for (int i = 0; i < activities.Length; i++)
            {
                activities[i].Actor = "editedActor" + activities[i].Actor;
                activities[i].Object = "editedObject" + activities[i].Object;
                activities[i].Verb = "editedVerb" + activities[i].Verb;
            }

            await this.UserFeed.UpdateActivitiesAsync(activities);

            var editedActivities = (await this.UserFeed.GetActivitiesAsync(0, 2)).Results.ToArray();
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            for (int i = 0; i < activities.Length; i++)
            {
                Assert.AreEqual(activities[i].Id, editedActivities[i].Id);
                Assert.AreEqual(activities[i].Actor, editedActivities[i].Actor);
                Assert.AreEqual(activities[i].Object, editedActivities[i].Object);
                Assert.AreEqual(activities[i].Verb, editedActivities[i].Verb);
            }
        }

        [Test]
        public async Task TestUpdateActivity()
        {
            var newActivity = new Activity("1", "test", "1")
            {
                ForeignId = "post:1",
                Time = DateTime.UtcNow,
            };
            newActivity.SetData<string>("myData", "1");
            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            first.Actor = "editedActor1";
            first.Object = "editedOject1";
            first.Verb = "editedVerbTest";
            first.SetData<string>("myData", "editedMyData1");

            await this.UserFeed.UpdateActivityAsync(first);

            activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var editedFirst = activities.First();
            Assert.AreEqual(first.Id, editedFirst.Id);
            Assert.AreEqual(first.GetData<string>("myData"), editedFirst.GetData<string>("myData"));
            Assert.AreEqual(first.Actor, editedFirst.Actor);
            Assert.AreEqual(first.Object, editedFirst.Object);
            Assert.AreEqual(first.Verb, editedFirst.Verb);
        }

        [Test]
        public async Task TestUpdateToTargets()
        {
            var fidTime = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);

            var targets = new List<string>()
            {
                "flat:" + Guid.NewGuid().ToString(),
                "user:" + Guid.NewGuid().ToString(),
            };

            var act = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime.ForeignId,
                Time = fidTime.Time,
                To = targets,
            };

            var insertedAct = await this.UserFeed.AddActivityAsync(act);
            Assert.AreEqual(2, insertedAct.To.Count);

            // add 1
            var add = "user:" + Guid.NewGuid().ToString();
            var updateResp = await this.UserFeed.UpdateActivityToTargetsAsync(fidTime, new[] { add });
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(1, updateResp.Added.Count);
            Assert.AreEqual(add, updateResp.Added[0]);
            Assert.AreEqual(3, updateResp.Activity.To.Count);
            Assert.IsNotNull(updateResp.Activity.To.ToList().Find(t => t == add));

            var updatedAct = (await this.UserFeed.GetActivitiesAsync(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(3, updatedAct.To.Count);
            Assert.IsNotNull(updatedAct.To.ToList().Find(t => t == add));

            // remove 1
            var remove = targets[0];
            updateResp = await this.UserFeed.UpdateActivityToTargetsAsync(insertedAct.Id, null, null, new[] { remove });
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(1, updateResp.Removed.Count);
            Assert.AreEqual(remove, updateResp.Removed[0]);
            Assert.AreEqual(2, updateResp.Activity.To.Count);
            Assert.IsNull(updateResp.Activity.To.ToList().Find(t => t == remove));

            updatedAct = (await this.UserFeed.GetActivitiesAsync(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(2, updatedAct.To.Count);
            Assert.IsNull(updatedAct.To.ToList().Find(t => t == remove));

            // new ones
            var newOnes = new List<string>()
            {
                "flat:" + Guid.NewGuid().ToString(),
                "user:" + Guid.NewGuid().ToString(),
            };
            updateResp = await this.UserFeed.UpdateActivityToTargetsAsync(fidTime, null, newOnes);
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(2, updateResp.Activity.To.Count);
            Assert.AreEqual(2, updateResp.Added.Count);
            Assert.AreEqual(2, updateResp.Added.ToList().FindAll(t => newOnes.Contains(t)).Count);
            updatedAct = (await this.UserFeed.GetActivitiesAsync(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(2, updatedAct.To.Count);
            Assert.AreEqual(2, updatedAct.To.ToList().FindAll(t => newOnes.Contains(t)).Count);
        }

        [Test]
        public async Task TestActivityPartialUpdateByForeignIDTime()
        {
            var fidTime = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);
            var act = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime.ForeignId,
                Time = fidTime.Time,
            };
            act.SetData("custom_thing", "12345");
            act.SetData("custom_thing2", "foobar");
            act.SetData("custom_thing3", "some thing");

            var insertedAct = await this.UserFeed.AddActivityAsync(act);
            Assert.IsNotNull(insertedAct);
            Assert.AreEqual("12345", insertedAct.GetData<string>("custom_thing"));
            Assert.AreEqual("foobar", insertedAct.GetData<string>("custom_thing2"));
            Assert.AreEqual("some thing", insertedAct.GetData<string>("custom_thing3"));

            var set = new Dictionary<string, object>
            {
                { "custom_thing", "abcdef" },
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.ActivityPartialUpdateAsync(null, fidTime, set);
            });

            var updatedAct = (await Client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual("abcdef", updatedAct.GetData<string>("custom_thing"));

            var unset = new[] { "custom_thing2" };

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.ActivityPartialUpdateAsync(null, fidTime, null, unset);
            });

            updatedAct = (await Client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing2"));

            set["custom_thing"] = "zyx";
            unset[0] = "custom_thing3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.ActivityPartialUpdateAsync(null, fidTime, set, unset);
            });

            updatedAct = (await Client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing3"));
            Assert.AreEqual("zyx", updatedAct.GetData<string>("custom_thing"));
        }

        [Test]
        public async Task TestActivityPartialUpdateByID()
        {
            var act = new Activity("upd", "test", "1")
            {
                ForeignId = System.Guid.NewGuid().ToString(),
                Time = DateTime.UtcNow,
            };
            act.SetData("custom_thing", "12345");
            act.SetData("custom_thing2", "foobar");
            act.SetData("custom_thing3", "some thing");

            var insertedAct = await this.UserFeed.AddActivityAsync(act);
            Assert.IsNotNull(insertedAct);
            Assert.AreEqual("12345", insertedAct.GetData<string>("custom_thing"));
            Assert.AreEqual("foobar", insertedAct.GetData<string>("custom_thing2"));
            Assert.AreEqual("some thing", insertedAct.GetData<string>("custom_thing3"));

            var set = new Dictionary<string, object>();
            set.Add("custom_thing", "abcdef");

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.ActivityPartialUpdateAsync(insertedAct.Id, null, set);
            });

            var updatedAct = (await Client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual("abcdef", updatedAct.GetData<string>("custom_thing"));

            var unset = new[] { "custom_thing2" };

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.ActivityPartialUpdateAsync(insertedAct.Id, null, null, unset);
            });

            updatedAct = (await Client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing2"));

            set["custom_thing"] = "zyx";
            unset[0] = "custom_thing3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.ActivityPartialUpdateAsync(insertedAct.Id, null, set, unset);
            });

            updatedAct = (await Client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing3"));
            Assert.AreEqual("zyx", updatedAct.GetData<string>("custom_thing"));
        }
    }
}
