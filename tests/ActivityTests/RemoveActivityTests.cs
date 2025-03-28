using NUnit.Framework;
using Stream.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class RemoveActivityTests : TestBase
    {
        [Test]
        public async Task TestRemoveActivity()
        {
            var newActivity = new Activity("1", "test", "1");
            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.FirstOrDefault();
            Assert.AreEqual(response.Id, first.Id);

            await this.UserFeed.RemoveActivityAsync(first.Id);

            var nextActivities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(nextActivities);
            Assert.IsFalse(nextActivities.Any(na => na.Id == first.Id));
        }

        [Test]
        public async Task TestRemoveActivityByForeignId()
        {
            var fid = "post:42";
            var newActivity = new Activity("1", "test", "1")
            {
                ForeignId = fid,
            };

            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual(fid, activities.First().ForeignId);

            await this.UserFeed.RemoveActivityAsync(fid, true);

            activities = (await Client.Batch.GetActivitiesByIdAsync([response.Id])).Results;
            Assert.AreEqual(0, activities.Count());
        }
    }
}