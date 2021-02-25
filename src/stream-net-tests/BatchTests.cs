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
        private Stream.IStreamClient _client;

        [SetUp]
        public void Setup()
        {
            _client = Credentials.Instance.Client;
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
                var activities = await this._client.Batch.GetActivities(new string[1], new Stream.ForeignIDTime[1]);
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
                await this._client.Batch.FollowMany(new Stream.Follow[] { new Stream.Follow("user:1", "user:2") }, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.FollowMany(new Stream.Follow[] { new Stream.Follow("user:1", "user:2") }, 1000);
            });
        }
    }
}
