using NUnit.Framework;
using Stream;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class UserTests : TestBase
    {
        [Test]
        public async Task TestUsers()
        {
            // Create user
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
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(userData, u.Data);

            Assert.ThrowsAsync<StreamException>(async () =>
            {
                u = await Client.Users.AddAsync(userId, userData);
            });

            var newUserData = new Dictionary<string, object>()
            {
                { "field", "othervalue" },
            };
            Assert.DoesNotThrowAsync(async () =>
            {
                u = await Client.Users.AddAsync(userId, newUserData, true);
            });
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(userData, u.Data);

            // Get user
            u = await Client.Users.GetAsync(userId);
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(userData, u.Data);

            // Update user
            u = await Client.Users.UpdateAsync(userId, newUserData);
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(newUserData, u.Data);

            // Delete user
            await Client.Users.DeleteAsync(userId);

            Assert.ThrowsAsync<StreamException>(async () =>
            {
                var x = await Client.Users.GetAsync(userId);
            });
        }

        [Test]
        public async Task TestForeignIdTimeBug()
        {
            // local credentials:
            const string api_key = "qk4nn7rpcn75";
            const string api_secret = "vz4yhgcwums5h64uh8xbx58c9yfgtkjneab5ch9skds93xzqd8mv6dc824dusnvt";

            Environment.SetEnvironmentVariable("STREAM_URL", "http://localhost:8000");

            var client = new StreamClient(api_key, api_secret);

            var activityId = "a20abf36-afd1-11ee-836a-ead473e55f11";
            var result = await client.Reactions.FilterAsync(ReactionFiltering.Default.WithRanking("popular"), ReactionPagination.By.ActivityId(activityId));
            Assert.NotNull(result);

        }
    }
}