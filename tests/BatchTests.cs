using NUnit.Framework;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class BatchTests : TestBase
    {
        [Test]
        public void TestUnfollowManyArgumentValidation()
        {
            // Should work with empty array
            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.UnfollowManyAsync(new UnfollowRelation[] { });
            });
            
            // Should work with valid unfollow relation objects
            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.UnfollowManyAsync(new[] { new UnfollowRelation("user:1", "user:2", false) });
            });

            // Should work with keepHistory true
            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.UnfollowManyAsync(new[] { new UnfollowRelation("user:1", "user:2", true) });
            });
        }

        [Test]
        public void TestGetEnrichedActivitiesArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var activities = await Client.Batch.GetEnrichedActivitiesAsync(ids: null);
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var activities = await Client.Batch.GetEnrichedActivitiesAsync(foreignIdTimes: null);
            });
        }

        [Test]
        public void TestFollowManyArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await Client.Batch.FollowManyAsync(null, -1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await Client.Batch.FollowManyAsync(null, 1001);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.FollowManyAsync(new[] { new Follow("user:1", "user:2") }, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.FollowManyAsync(new[] { new Follow("user:1", "user:2") }, 1000);
            });
        }

        [Test]
        public async Task TestAddToMany()
        {
            var newActivity = new Activity("1", "test", "1");
            await Client.Batch.AddToManyAsync(newActivity, new[] { UserFeed, UserFeed2 });

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(newActivity.Actor, first.Actor);
            Assert.AreEqual(newActivity.Object, first.Object);
            Assert.AreEqual(newActivity.Verb, first.Verb);

            activities = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            first = activities.First();
            Assert.AreEqual(newActivity.Actor, first.Actor);
            Assert.AreEqual(newActivity.Object, first.Object);
            Assert.AreEqual(newActivity.Verb, first.Verb);
        }

        [Test]
        public async Task TestBatchFollow()
        {
            await Client.Batch.FollowManyAsync(new[]
            {
                new Follow(UserFeed, FlatFeed),
                new Follow(UserFeed2, FlatFeed),
            });

            var newActivity = new Activity("1", "test", "1");
            var response = await this.FlatFeed.AddActivityAsync(newActivity);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);

            activities = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);
        }

        [Test]
        public async Task TestBatchFollowWithCopyLimit()
        {
            await Client.Batch.FollowManyAsync(new[]
            {
                new Follow(this.UserFeed, this.FlatFeed),
                new Follow(this.UserFeed2, this.FlatFeed),
            }, 10);

            var newActivity = new Activity("1", "test", "1");
            var response = await this.FlatFeed.AddActivityAsync(newActivity);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);

            activities = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);
        }

        [Test]
        public async Task TestBatchPartialUpdate()
        {
            var fidTime1 = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);
            var act1 = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime1.ForeignId,
                Time = fidTime1.Time,
            };
            act1.SetData("custom_thing", "12345");
            act1.SetData("custom_thing2", "foobar");
            act1.SetData("custom_thing3", "some thing");
            var fidTime2 = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-3));
            var act2 = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime2.ForeignId,
                Time = fidTime2.Time,
            };
            act2.SetData("custom_flag", "val1");
            act2.SetData("custom_flag2", "val2");
            act2.SetData("custom_flag3", "val3");

            var fidTime3 = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-6));
            var act3 = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime3.ForeignId,
                Time = fidTime3.Time,
            };
            var customData = new Dictionary<string, string>()
            {
                { "name", "BOB" },
                { "address", "90210" },
                { "email", "bob@bobobo.com" },
            };
            act3.SetData("details", customData);

            var response = (await this.UserFeed.AddActivitiesAsync(new[] { act1, act2, act3 })).Activities;
            var insertedActs = response.ToArray();

            // FID TIME
            var upd1 = new ActivityPartialUpdateRequestObject()
            {
                ForeignId = fidTime1.ForeignId,
                Time = fidTime1.Time,
                Unset = new[] { "custom_thing3" },
            };

            var set = new Dictionary<string, object>
            {
                { "details.address", "nowhere" },
            };

            var upd2 = new ActivityPartialUpdateRequestObject()
            {
                ForeignId = fidTime3.ForeignId,
                Time = fidTime3.Time,
                Set = set,
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.ActivitiesPartialUpdateAsync(new[] { upd1, upd2 });
            });

            var updatedActs = (await this.UserFeed.GetActivitiesAsync()).Results.ToArray();

            Assert.IsNull(updatedActs[0].GetData<string>("custom_thing3"));
            var extraData = updatedActs[2].GetData<Dictionary<string, string>>("details");
            Assert.AreEqual("nowhere", extraData["address"]);

            // ID
            set.Clear();
            set.Add("custom_flag2", "foobar");
            upd1 = new ActivityPartialUpdateRequestObject
            {
                Id = insertedActs[1].Id,
                Set = set,
            };
            upd2 = new ActivityPartialUpdateRequestObject
            {
                Id = insertedActs[2].Id,
                Unset = new[] { "details.name" },
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.ActivitiesPartialUpdateAsync(new[] { upd1, upd2 });
            });

            updatedActs = (await this.UserFeed.GetActivitiesAsync()).Results.ToArray();

            Assert.AreEqual("foobar", updatedActs[1].GetData<string>("custom_flag2"));
            extraData = updatedActs[2].GetData<Dictionary<string, string>>("details");
            Assert.False(extraData.ContainsKey("name"));
        }

        [Test]
        public async Task TestBatchUpdateActivity()
        {
            var activity = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny",
            };
            activity.SetData("custom", "field");
            var insertedActivity = await this.UserFeed.AddActivityAsync(activity);

            activity.Target = "timmy";
            activity.SetData("custom", "data");
            activity.SetData("another", "thing");

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.UpdateActivityAsync(activity);
            });

            var updatedActivity = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results.FirstOrDefault();
            Assert.NotNull(updatedActivity);
            Assert.AreEqual(insertedActivity.Id, updatedActivity.Id);
            Assert.AreEqual(activity.Target, updatedActivity.Target);
            Assert.AreEqual(activity.GetData<string>("custom"), updatedActivity.GetData<string>("custom"));
            Assert.AreEqual(activity.GetData<string>("another"), updatedActivity.GetData<string>("another"));
        }

        [Test]
        public async Task TestBatchUpdateActivities()
        {
            var activity = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny",
            };
            activity.SetData("custom", "field");
            var activity2 = new Activity("user:123", "posts", "selfie")
            {
                ForeignId = "selfie:2",
                Time = DateTime.UtcNow,
            };

            var insertedActivity = await this.UserFeed.AddActivityAsync(activity);
            var insertedActivity2 = await this.FlatFeed.AddActivityAsync(activity2);

            activity.SetData("custom", "data");
            activity.Target = null;
            activity2.SetData("new-stuff", new int[] { 3, 2, 1 });
            activity2.Actor = "user:3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Batch.UpdateActivitiesAsync(new[] { activity, activity2 });
            });

            var updatedActivity = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results.FirstOrDefault();
            Assert.NotNull(updatedActivity);
            Assert.AreEqual(insertedActivity.Id, updatedActivity.Id);
            Assert.AreEqual(string.Empty, updatedActivity.Target);
            Assert.AreEqual(activity.GetData<string>("custom"), updatedActivity.GetData<string>("custom"));

            var updatedActivity2 = (await this.FlatFeed.GetActivitiesAsync(0, 1)).Results.FirstOrDefault();
            Assert.NotNull(updatedActivity2);
            Assert.AreEqual(insertedActivity2.Id, updatedActivity2.Id);
            Assert.AreEqual(activity2.Actor, updatedActivity2.Actor);
            Assert.AreEqual(activity2.GetData<int[]>("custom"), updatedActivity2.GetData<int[]>("custom"));
        }

        [Test]
        public async Task TestBatchActivityForeignIdTime()
        {
            var activity = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny",
            };

            var insertedActivity = await this.UserFeed.AddActivityAsync(activity);

            var foreignIdTime = new ForeignIdTime(insertedActivity.ForeignId, insertedActivity.Time.Value);
            IEnumerable<ForeignIdTime> foreignIdTimes = new ForeignIdTime[] { foreignIdTime };

            GenericGetResponse<Activity> result = await Client.Batch.GetActivitiesByForeignIdAsync(foreignIdTimes);

            Assert.AreEqual(1, result.Results.Count);
        }

        [Test]
        public async Task TestBatchUnfollowManyKeepHistoryFalse()
        {
            // First set up follows and add activities
            await Client.Batch.FollowManyAsync(new[]
            {
                new Follow(UserFeed, FlatFeed),
                new Follow(UserFeed2, FlatFeed),
            });

            var newActivity = new Activity("1", "test", "1");
            var response = await this.FlatFeed.AddActivityAsync(newActivity);

            // Verify follows are working
            var activities1 = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            var activities2 = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            
            Assert.IsNotNull(activities1);
            Assert.AreEqual(1, activities1.Count());
            Assert.AreEqual(response.Id, activities1.First().Id);
            
            Assert.IsNotNull(activities2);
            Assert.AreEqual(1, activities2.Count());
            Assert.AreEqual(response.Id, activities2.First().Id);

            // Use UnfollowMany with keepHistory=false
            await Client.Batch.UnfollowManyAsync(new[]
            {
                new UnfollowRelation(UserFeed, FlatFeed, false),
                new UnfollowRelation(UserFeed2, FlatFeed, false),
            });

            // Verify activities are removed
            activities1 = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            activities2 = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            
            Assert.IsNotNull(activities1);
            Assert.AreEqual(0, activities1.Count());
            
            Assert.IsNotNull(activities2);
            Assert.AreEqual(0, activities2.Count());
        }

        [Test]
        public async Task TestBatchUnfollowManyKeepHistoryTrue()
        {
            // First set up follows and add activities
            await Client.Batch.FollowManyAsync(new[]
            {
                new Follow(UserFeed, FlatFeed),
                new Follow(UserFeed2, FlatFeed),
            });

            var newActivity = new Activity("1", "test", "1");
            var response = await this.FlatFeed.AddActivityAsync(newActivity);

            // Verify follows are working
            var activities1 = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            var activities2 = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            
            Assert.IsNotNull(activities1);
            Assert.AreEqual(1, activities1.Count());
            Assert.AreEqual(response.Id, activities1.First().Id);
            
            Assert.IsNotNull(activities2);
            Assert.AreEqual(1, activities2.Count());
            Assert.AreEqual(response.Id, activities2.First().Id);

            // Use UnfollowMany with keepHistory=true
            await Client.Batch.UnfollowManyAsync(new[]
            {
                new UnfollowRelation(UserFeed, FlatFeed, true),
                new UnfollowRelation(UserFeed2, FlatFeed, true),
            });

            // Verify activities are retained
            activities1 = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            activities2 = (await this.UserFeed2.GetActivitiesAsync(0, 1)).Results;
            
            Assert.IsNotNull(activities1);
            Assert.AreEqual(1, activities1.Count());
            Assert.AreEqual(response.Id, activities1.First().Id);
            
            Assert.IsNotNull(activities2);
            Assert.AreEqual(1, activities2.Count());
            Assert.AreEqual(response.Id, activities2.First().Id);
        }
    }
}
