using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class BatchTests
    {
        private GetStream.IStreamClient _client;

        [SetUp]
        public void Setup()
        {
            _client = new GetStream.StreamClient(
                "98a6bhskrrwj",
                "t3nj7j8m6dtdbbakzbu9p7akjk5da8an5wxwyt6g73nt5hf9yujp8h4jw244r67p",
                new GetStream.StreamClientOptions()
                {
                    Location = GetStream.StreamApiLocation.USEast
                });
        }

        [Test]

        public void TestGetActivitiesArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var activities = await this._client.Batch.GetActivities();
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var activities = await this._client.Batch.GetActivities(new string[1], new GetStream.ForeignIDTime[1]);
            });
        }

        [Test]

        public void TestFollowManyArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await this._client.Batch.FollowMany(null, -1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await this._client.Batch.FollowMany(null, 1001);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.FollowMany(new GetStream.Follow[] { new GetStream.Follow("user:1", "user:2") }, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.FollowMany(new GetStream.Follow[] { new GetStream.Follow("user:1", "user:2") }, 1000);
            });
        }
    }
}
