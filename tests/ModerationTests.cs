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