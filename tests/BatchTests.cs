using NUnit.Framework;
using Stream;
using Stream.Models;
using System;

namespace StreamNetTests
{
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
        public void TestGetEnrichedActivitiesArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var activities = await _client.Batch.GetEnrichedActivitiesAsync(ids: null);
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var activities = await _client.Batch.GetEnrichedActivitiesAsync(foreignIdTimes: null);
            });
        }

        [Test]

        public void TestFollowManyArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _client.Batch.FollowManyAsync(null, -1);
            });
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _client.Batch.FollowManyAsync(null, 1001);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Batch.FollowManyAsync(new[] { new Follow("user:1", "user:2") }, 0);
            });
            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Batch.FollowManyAsync(new[] { new Follow("user:1", "user:2") }, 1000);
            });
        }
    }
}
