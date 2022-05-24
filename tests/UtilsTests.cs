using NUnit.Framework;
using Stream.Models;
using Stream.Utils;
using System;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class UtilsTests : TestBase
    {
        [Test]
        public void TestIdGenerator()
        {
            // All these test cases are copied from the backend Go implementation
            var first = ActivityIdGenerator.GenerateId("123sdsd333}}}", 1451260800);
            Assert.AreEqual("f01c0000-acf5-11e5-8080-80006a8b5bc2", first.ToString());

            var second = ActivityIdGenerator.GenerateId("6621934", 1452862482);
            Assert.AreEqual("24f9d500-bb87-11e5-8080-80012ce3cc51", second.ToString());

            var third = ActivityIdGenerator.GenerateId("6621938", 1452862489);
            Assert.AreEqual("2925f280-bb87-11e5-8080-8001597d791f", third.ToString());

            var fourth = ActivityIdGenerator.GenerateId("6621941", 1452862492);
            Assert.AreEqual("2aefb600-bb87-11e5-8080-8001226fe2f2", fourth.ToString());

            var fifth = ActivityIdGenerator.GenerateId("6621945", 1452862496);
            Assert.AreEqual("2d521000-bb87-11e5-8080-800026259780", fifth.ToString());

            var sixth = ActivityIdGenerator.GenerateId("top_issues_summary_557dc1d9e46fea0a4c000002", 1463914800);
            Assert.AreEqual("53dbf800-200c-11e6-8080-800023ec2877", sixth.ToString());

            var seventh = ActivityIdGenerator.GenerateId("6625387", 1452866055);
            Assert.AreEqual("76a65d80-bb8f-11e5-8080-8000530672db", seventh.ToString());

            var eight = ActivityIdGenerator.GenerateId("UserPrediction:11127278", 1480174117);
            Assert.AreEqual("00000080-b3ed-11e6-8080-8000444ef374", eight.ToString());

            var nineth = ActivityIdGenerator.GenerateId("467791-42-follow", 1481475921);
            Assert.AreEqual("ffa65e80-bfc3-11e6-8080-800027086507", nineth.ToString());
        }

        [Test]
        public async Task TestActivityIdSameAsBackend()
        {
            var time = DateTime.UtcNow;
            var foreignId = Guid.NewGuid().ToString();
            var inputAct = new Activity("1", "test", "1") { Time = time, ForeignId = foreignId };
            var activity = await this.UserFeed.AddActivityAsync(inputAct);

            Assert.AreEqual(ActivityIdGenerator.GenerateId(activity.ForeignId, time).ToString(), activity.Id);
        }
    }
}
