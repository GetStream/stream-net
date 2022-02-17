using Newtonsoft.Json;
using NUnit.Framework;
using Stream;
using Stream.Models;
using Stream.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace StreamNetTests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ClientTests
    {
        private Stream.IStreamClient _client;

        [SetUp]
        public void Setup()
        {
            _client = Credentials.Instance.Client;
        }

        [Test]

        public void TestClientArgumentsValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new Stream.StreamClient("", "asfd");
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new Stream.StreamClient("asdf", null);
            });
        }

        [Test]

        public void TestFeedIdValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var feed = _client.Feed(null, null);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var feed = _client.Feed("flat", "");
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var feed = _client.Feed("", "1");
            });
            Assert.Throws<ArgumentException>(() =>
            {
                var feed = _client.Feed("flat:1", "2");
            });
            Assert.Throws<ArgumentException>(() =>
            {
                var feed = _client.Feed("flat 1", "2");
            });
            Assert.Throws<ArgumentException>(() =>
            {
                var feed = _client.Feed("flat1", "2:3");
            });
        }

        [Test]
        public void TestFollowFeedIdValidation()
        {
            var user1 = _client.Feed("user", "11");

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await user1.FollowFeedAsync(null, null);
            });
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await user1.FollowFeedAsync("flat", "");
            });
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await user1.FollowFeedAsync("", "1");
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await user1.FollowFeedAsync("flat:1", "2");
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await user1.FollowFeedAsync("flat 1", "2");
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await user1.FollowFeedAsync("flat1", "2:3");
            });
        }

        [Test]
        public void TestActivityPartialUpdateArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _client.ActivityPartialUpdateAsync();
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _client.ActivityPartialUpdateAsync("id", new ForeignIdTime("fid", DateTime.Now));
            });
        }

        [Test]
        public void TestToken()
        {
            var result = DecodeJWT(_client.CreateUserToken("user"));
            Assert.AreEqual("user", (string)result["user_id"]);

            var extra = new Dictionary<string, object>()
            {
                {"client","dotnet"},
                {"testing", true}
            };
            result = DecodeJWT(_client.CreateUserToken("user2", extra));

            Assert.AreEqual("user2", (string)result["user_id"]);
            Assert.AreEqual("dotnet", (string)result["client"]);
            Assert.AreEqual(true, (bool)result["testing"]);
            Assert.False(result.ContainsKey("missing"));
        }

        private Dictionary<string, object> DecodeJWT(string token)
        {
            var segment = token.Split('.')[1];
            var mod = segment.Length % 4;
            if (mod > 0)
            {
                segment += "".PadLeft(4 - mod, '=');
            }
            var encoded = Convert.FromBase64String(segment.Replace('-', '+').Replace('_', '/'));
            var payload = Encoding.UTF8.GetString(encoded);
            return StreamJsonConverter.DeserializeObject<Dictionary<string, object>>(payload);
        }
    }
}
