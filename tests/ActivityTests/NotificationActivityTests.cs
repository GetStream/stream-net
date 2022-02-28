using NUnit.Framework;
using Stream.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class NotificationActivityTests : TestBase
    {
        [Test]
        public async Task TestMarkRead()
        {
            var newActivity = new Activity("1", "tweet", "1");
            var first = await NotificationFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "run", "2");
            var second = await NotificationFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "share", "3");
            var third = await NotificationFeed.AddActivityAsync(newActivity);

            var activities = (await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(2).WithMarker(ActivityMarker.Mark().AllRead()))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            activities = (await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(2).WithMarker(ActivityMarker.Mark().AllRead()))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);
        }

        [Test]
        public async Task TestMarkReadByIds()
        {
            var newActivity = new Activity("1", "tweet", "1");
            var first = await NotificationFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "run", "2");
            var second = await NotificationFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "share", "3");
            var third = await NotificationFeed.AddActivityAsync(newActivity);

            var activities = (await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(2))).Results;

            var marker = ActivityMarker.Mark();
            foreach (var activity in activities)
            {
                marker = marker.Read(activity.Id);
            }

            var notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            activities = (await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(3).WithMarker(marker))).Results;

            activities = (await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(3))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(3, activities.Count());

            notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(2).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);
        }

        [Test]
        public async Task TestMarkNotificationsRead()
        {
            var newActivity = new Activity("1", "tweet", "1");
            var first = await NotificationFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "run", "2");
            var second = await NotificationFeed.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "share", "3");
            var third = await NotificationFeed.AddActivityAsync(newActivity);

            var response = await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(2).WithMarker(ActivityMarker.Mark().AllRead()));
            Assert.IsNotNull(response);

            var activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            response = await NotificationFeed.GetNotificationActivities(GetOptions.Default.WithLimit(2));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Unread);

            activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);
        }
    }
}