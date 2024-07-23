using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Stream;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
namespace StreamNetTests
{
    [TestFixture]
    public class ReactionTests : TestBase
    {
        [Test]
        public async Task TestReactions()
        {
            var a = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny",
            };

            var activity = await this.UserFeed.AddActivityAsync(a);

            var data = new Dictionary<string, object>() { { "field", "value" }, { "number", 2 }, };

            var r = await Client.Reactions.AddAsync("like", activity.Id, "bobby", data);

            Assert.NotNull(r);
            Assert.AreEqual(r.ActivityId, activity.Id);
            Assert.AreEqual(r.Kind, "like");
            Assert.AreEqual(r.UserId, "bobby");
            Assert.AreEqual(r.Data, data);
            Assert.True(r.CreatedAt.HasValue);
            Assert.True(r.UpdatedAt.HasValue);
            Assert.IsNotEmpty(r.Id);

            // get reaction
            Reaction r2 = null;
            Assert.DoesNotThrowAsync(async () => r2 = await Client.Reactions.GetAsync(r.Id));

            Assert.NotNull(r2);
            Assert.AreEqual(r2.ActivityId, r.ActivityId);
            Assert.AreEqual(r2.Kind, "like");
            Assert.AreEqual(r2.UserId, "bobby");
            Assert.AreEqual(r2.Data, r.Data);
            Assert.AreEqual(r2.Id, r.Id);

            // Update reaction
            data["number"] = 321;
            data["new"] = "field";
            data.Remove("field");

            var beforeTime = r.UpdatedAt.Value;
            Assert.DoesNotThrowAsync(async () => r2 = await Client.Reactions.UpdateAsync(r.Id, data));
            Assert.NotNull(r2);
            Assert.False(r2.Data.ContainsKey("field"));
            Assert.True(r2.Data.TryGetValue("number", out object n));
            Assert.AreEqual((long)n, 321);
            Assert.True(r2.Data.ContainsKey("new"));

            // Add children
            var c1 = await Client.Reactions.AddChildAsync(r, "upvote", "tommy");
            var c2 = await Client.Reactions.AddChildAsync(r, "downvote", "timmy");
            var c3 = await Client.Reactions.AddChildAsync(r.Id, "upvote", "jimmy");

            var parent = await Client.Reactions.GetAsync(r.Id);

            Assert.AreEqual(parent.ChildrenCounters["upvote"], 2);
            Assert.AreEqual(parent.ChildrenCounters["downvote"], 1);

            Assert.IsTrue(parent.LatestChildren["upvote"].Select(x => x.Id).Contains(c1.Id));
            Assert.IsTrue(parent.LatestChildren["upvote"].Select(x => x.Id).Contains(c3.Id));
            Assert.IsTrue(parent.LatestChildren["downvote"].Select(x => x.Id).Contains(c2.Id));

            // restore tests once there is support on server
            // Assert.DoesNotThrowAsync(async () => await Client.Reactions.DeleteAsync(r.Id, true));
            // Assert.DoesNotThrowAsync(async () => await Client.Reactions.RestoreSoftDeletedAsync(r.Id));
            Assert.DoesNotThrowAsync(async () => await Client.Reactions.DeleteAsync(r.Id));

            Assert.ThrowsAsync<StreamException>(async () => await Client.Reactions.GetAsync(r.Id));
        }

        [Test]
        public async Task TestReactionModeration()
        {
            var a = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny",
            };

            var activity = await this.UserFeed.AddActivityAsync(a);

            var data = new Dictionary<string, object>() { { "field", "value" }, { "number", 2 }, { "text", "pissoar" }, };

            var r = await Client.Reactions.AddAsync("like", activity.Id, "bobby", data, null, "moderation_config_1_reaction");

            Assert.NotNull(r);
            Assert.AreEqual(r.ActivityId, activity.Id);
            Assert.AreEqual(r.Kind, "like");
            Assert.AreEqual(r.UserId, "bobby");
            Assert.AreEqual(r.Data, data);

            var response = r.GetModerationResponse();

            Assert.AreEqual("complete", response.Status);
            Assert.AreEqual("remove", response.RecommendedAction);

            Assert.ThrowsAsync<StreamException>(async () => await Client.Reactions.GetAsync(r.Id));
        }

        [Test]
        public async Task TestReactionPagination()
        {
            var a = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny",
            };

            var activity = await this.UserFeed.AddActivityAsync(a);

            a.Time = DateTime.UtcNow;
            a.ForeignId = "cake:123";
            var activity2 = await this.UserFeed.AddActivityAsync(a);

            var data = new Dictionary<string, object>() { { "field", "value" }, { "number", 2 }, };

            var userId = Guid.NewGuid().ToString();

            var r1 = await Client.Reactions.AddAsync("like", activity.Id, userId, data);
            var r2 = await Client.Reactions.AddAsync("comment", activity.Id, userId, data);
            var r3 = await Client.Reactions.AddAsync("like", activity.Id, "bob", data);

            var r4 = await Client.Reactions.AddChildAsync(r3, "upvote", "tom", data);
            var r5 = await Client.Reactions.AddChildAsync(
                r3.Id,
                Guid.NewGuid().ToString(),
                "upvote",
                "mary",
                data);

            // activity id
            var filter = ReactionFiltering.Default;
            var pagination = ReactionPagination.By.ActivityId(activity.Id).Kind("like");

            var reactionsByActivity = await Client.Reactions.FilterAsync(filter, pagination);
            Assert.AreEqual(2, reactionsByActivity.Count());

            var r = (List<Reaction>)reactionsByActivity;
            var actual = r.Find(x => x.Id == r1.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r1.Id, actual.Id);
            Assert.AreEqual(r1.Kind, actual.Kind);
            Assert.AreEqual(r1.ActivityId, actual.ActivityId);

            actual = r.Find(x => x.Id == r3.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r3.Id, actual.Id);
            Assert.AreEqual(r3.Kind, actual.Kind);
            Assert.AreEqual(r3.ActivityId, actual.ActivityId);

            // with limit
            reactionsByActivity = await Client.Reactions.FilterAsync(
                filter.WithLimit(1),
                pagination);
            Assert.AreEqual(1, reactionsByActivity.Count());

            // with data
            var reactionsByActivityWithData = await Client.Reactions.FilterWithActivityAsync(
                filter.WithLimit(1),
                pagination);
            Assert.AreEqual(1, reactionsByActivity.Count());
            Assert.AreEqual(data, reactionsByActivity.FirstOrDefault().Data);

            // user id
            filter = ReactionFiltering.Default;
            pagination = ReactionPagination.By.UserId(userId);

            var reactionsByUser = await Client.Reactions.FilterAsync(filter, pagination);
            Assert.AreEqual(2, reactionsByUser.Count());

            r = (List<Reaction>)reactionsByUser;
            actual = r.Find(x => x.Id == r1.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r1.Id, actual.Id);
            Assert.AreEqual(r1.Kind, actual.Kind);
            Assert.AreEqual(r1.ActivityId, actual.ActivityId);

            actual = r.Find(x => x.Id == r2.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r2.Id, actual.Id);
            Assert.AreEqual(r2.Kind, actual.Kind);
            Assert.AreEqual(r2.ActivityId, actual.ActivityId);

            // reaction id
            filter = ReactionFiltering.Default;
            pagination = ReactionPagination.By.Kind("upvote").ReactionId(r3.Id);

            var reactionsByParent = await Client.Reactions.FilterAsync(filter, pagination);
            Assert.AreEqual(2, reactionsByParent.Count());

            r = reactionsByParent.ToList();
            actual = r.Find(x => x.Id == r4.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r4.Id, actual.Id);
            Assert.AreEqual(r4.Kind, actual.Kind);
            Assert.AreEqual(r4.ActivityId, actual.ActivityId);
            Assert.AreEqual(r4.UserId, actual.UserId);

            actual = r.Find(x => x.Id == r5.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r5.Id, actual.Id);
            Assert.AreEqual(r5.Kind, actual.Kind);
            Assert.AreEqual(r5.ActivityId, actual.ActivityId);
            Assert.AreEqual(r5.UserId, actual.UserId);
        }
    }
}