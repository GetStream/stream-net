using NUnit.Framework;
using Stream;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamNetTests
{
    using Stream.Models;

    [TestFixture]
    public class ModerationTests : TestBase
    {
        [Test]
        public async Task TestModerationTemplate()
        {
            var newActivity2 = new Activity("1", "test", "2")
            {
                ForeignId = "r-test-2",
                Time = DateTime.Parse("2000-08-17T16:32:32"),
            };
            newActivity2.SetData("moderation_template", "moderation_template_test_images");

            newActivity2.SetData("text", "pissoar");

            var attachments = new Dictionary<string, object>();
            string[] images = new string[] { "image1", "image2" };
            attachments["images"] = images;

            newActivity2.SetData("attachment", attachments);

            var response = await this.UserFeed.AddActivityAsync(newActivity2);

            var modResponse = response.GetData<ModerationResponse>("moderation");

            Assert.AreEqual(modResponse.Status, "complete");
            Assert.AreEqual(modResponse.RecommendedAction, "remove");
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

            var updatedData = new Dictionary<string, object>() { { "field", "updated" }, { "number", 3 }, { "text", "pissoar" }, };
            var updatedReaction = await Client.Reactions.UpdateAsync(r.Id, updatedData, null, "moderation_config_1_reaction");

            Assert.NotNull(updatedReaction);
            Assert.AreEqual(updatedReaction.Id, r.Id);
            Assert.AreEqual(updatedReaction.Data["field"], "updated");
            Assert.AreEqual(updatedReaction.Data["number"], 3);

            var updatedResponse = updatedReaction.GetModerationResponse();
            Assert.AreEqual("complete", updatedResponse.Status);
            Assert.AreEqual("remove", updatedResponse.RecommendedAction);

            var updatedData2 = new Dictionary<string, object>() { { "field", "updated" }, { "number", 3 }, { "text", "hello" }, };
            var updatedReaction2 = await Client.Reactions.UpdateAsync(r.Id, updatedData2, null, "moderation_config_1_reaction");

            Assert.NotNull(updatedReaction2);
            Assert.AreEqual(updatedReaction2.Id, r.Id);
            Assert.AreEqual(updatedReaction2.Data["field"], "updated");
            Assert.AreEqual(updatedReaction2.Data["number"], 3);

            var updatedResponse2 = updatedReaction2.GetModerationResponse();
            Assert.AreEqual("complete", updatedResponse2.Status);
            Assert.AreEqual("remove", updatedResponse2.RecommendedAction);

            var c1 = await Client.Reactions.AddChildAsync(r.Id, "upvote", "tommy", updatedData, null, "moderation_config_1_reaction");
            Assert.NotNull(c1);

            var updatedResponse2 = c1.GetModerationResponse();
            Assert.AreEqual("complete", c1.Status);
            Assert.AreEqual("remove", c1.RecommendedAction);
        }

        [Test]
        public async Task TestFlagUser()
        {
            var userId = "flagginguser";
            var userData = new Dictionary<string, object>
            {
                { "field", "value" },
                { "is_admin", true },
            };

            var u = await Client.Users.AddAsync(userId, userData, true);

            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);

            var response = await Client.Moderation.FlagUserAsync(userId, "flagged-user", "blood");
            Assert.NotNull(response);
        }

        [Test]
        public async Task TestFlagUserError()
        {
            Assert.ThrowsAsync<StreamException>(async () => await Client.Moderation.FlagUserAsync("", string.Empty, "blood"));
        }

        [Test]
        public async Task TestFlagActivity()
        {
            var newActivity = new Activity("vishal", "test", "1");
            newActivity.SetData<string>("stringint", "42");
            newActivity.SetData<string>("stringdouble", "42.2");
            newActivity.SetData<string>("stringcomplex", "{ \"test1\": 1, \"test2\": \"testing\" }");
            
            // Set moderation data with origin_feed
            var moderationData = new Dictionary<string, object>
            {
                { "origin_feed", this.UserFeed.FeedId }
            };
            newActivity.SetData("moderation", moderationData);

            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var response1 = await Client.Moderation.FlagActivityAsync(response.Id, response.Actor, "blood");

            Assert.NotNull(response1);
        }

        [Test]
        public async Task TestFlagReaction()
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

            var response = await Client.Moderation.FlagReactionAsync(r.Id, r.UserId, "blood");
            Assert.NotNull(response);
        }
    }
}