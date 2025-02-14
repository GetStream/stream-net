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

        }

        [Test]
        public async Task TestFlagUser()
        {
            var userId = Guid.NewGuid().ToString();
            var userData = new Dictionary<string, object>
            {
                { "field", "value" },
                { "is_admin", true },
            };

            var u = await Client.Users.AddAsync(userId, userData);

            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);

            var response = await Client.Moderation.FlagUserAsync(userId, "blood");
            Assert.NotNull(response);
        }

        [Test]
        public async Task TestFlagActivity()
        {
            var newActivity = new Activity("vishal", "test", "1");
            newActivity.SetData<string>("stringint", "42");
            newActivity.SetData<string>("stringdouble", "42.2");
            newActivity.SetData<string>("stringcomplex", "{ \"test1\": 1, \"test2\": \"testing\" }");

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