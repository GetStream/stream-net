using NUnit.Framework;
using Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class UsersBatchTests : TestBase
    {
        [Test]
        public async Task TestAddGetUsersAsync()
        {
            var userIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            var users = new List<User>
            {
                new User { Id = userIds[0], Data = new Dictionary<string, object> { { "field", "value1" } } },
                new User { Id = userIds[1], Data = new Dictionary<string, object> { { "field", "value2" } } },
            };

            var response = await Client.UsersBatch.UpsertUsersAsync(users);

            Assert.NotNull(response);
            Assert.AreEqual(users.Count, response.Count());

            var usersReturned = await Client.UsersBatch.GetUsersAsync(userIds);

            Assert.NotNull(usersReturned);
            Assert.AreEqual(userIds.Count, usersReturned.Count());
        }

        // [Test]
        //        public async Task TestUpdateUsersAsync()
        //        {
        //            var users = new List<User>
        //            {
        //                new User { Id = Guid.NewGuid().ToString(), Data = new Dictionary<string, object> { { "field", "newvalue1" } } },
        //                new User { Id = Guid.NewGuid().ToString(), Data = new Dictionary<string, object> { { "field", "newvalue2" } } },
        //            };
        //
        //            var response = await Client.UsersBatch.UpdateUsersAsync(users);
        //
        //            Assert.NotNull(response);
        //            Assert.AreEqual(users.Count, response.Count());
        //        }
        [Test]
        public async Task TestDeleteUsersAsync()
        {
            var userIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            var deletedUserIds = await Client.UsersBatch.DeleteUsersAsync(userIds);

            Assert.NotNull(deletedUserIds);
            Assert.AreEqual(userIds.Count, deletedUserIds.Count());
            Assert.IsTrue(userIds.All(id => deletedUserIds.Contains(id)));
        }

        [Test]
        public async Task AddGetDeleteGetUsersAsync()
        {
            var userIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            // Add users
            var users = new List<User>
            {
                new User { Id = userIds[0], Data = new Dictionary<string, object> { { "field", "value1" } } },
                new User { Id = userIds[1], Data = new Dictionary<string, object> { { "field", "value2" } } },
            };

            var addResponse = await Client.UsersBatch.UpsertUsersAsync(users);
            Assert.NotNull(addResponse);
            Assert.AreEqual(users.Count, addResponse.Count());

            // Get users to confirm they were added
            var getUsersResponse = await Client.UsersBatch.GetUsersAsync(userIds);
            Assert.NotNull(getUsersResponse);
            Assert.AreEqual(users.Count, getUsersResponse.Count());

            // Delete users
            var deleteResponse = await Client.UsersBatch.DeleteUsersAsync(userIds);
            Assert.NotNull(deleteResponse);
            Assert.AreEqual(userIds.Count(), deleteResponse.Count());
            Assert.IsTrue(userIds.All(id => deleteResponse.Contains(id)));

            // Attempt to get deleted users to confirm they were deleted
            var getDeletedUsersResponse = await Client.UsersBatch.GetUsersAsync(userIds);
            Assert.IsEmpty(getDeletedUsersResponse);
        }
    }
}