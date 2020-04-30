using Microsoft.IdentityModel.Tokens.JWT;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ClientTests
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
        public void TestClientArgumentsValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new GetStream.StreamClient("", "asfd");
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new GetStream.StreamClient("asdf", null);
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

            Assert.Throws<ArgumentNullException>(() =>
            {
                user1.FollowFeed(null, null).GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                user1.FollowFeed("flat", "").GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                user1.FollowFeed("", "1").GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentException>(() =>
            {
                user1.FollowFeed("flat:1", "2").GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentException>(() =>
            {
                user1.FollowFeed("flat 1", "2").GetAwaiter().GetResult();
            });
            Assert.Throws<ArgumentException>(() =>
            {
                user1.FollowFeed("flat1", "2:3").GetAwaiter().GetResult();
            });
        }

        [Test]
        public void TestActivityPartialUpdateArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _client.ActivityPartialUpdate();
            });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _client.ActivityPartialUpdate("id", new GetStream.ForeignIDTime("fid", DateTime.Now));
            });
        }

        [Test]
        public void TestSessionToken()
        {
            var tokenString = _client.CreateUserSessionToken("user");
            var tok = new JWTSecurityToken(tokenString);
            object actualUserID;
            Assert.True(tok.Payload.TryGetValue("user_id", out actualUserID));
            Assert.AreEqual("user", (string)actualUserID);

            var extra = new Dictionary<string, object>()
            {
                {"client","dotnet"},
                {"testing", true}
            };
            tokenString = _client.CreateUserSessionToken("user2", extra);
            tok = new JWTSecurityToken(tokenString);

            object data;

            Assert.True(tok.Payload.TryGetValue("user_id", out data));
            Assert.AreEqual("user2", (string)data);
            Assert.True(tok.Payload.TryGetValue("client", out data));
            Assert.AreEqual("dotnet", (string)data);
            Assert.True(tok.Payload.TryGetValue("testing", out data));
            Assert.AreEqual(true, (bool)data);

            Assert.False(tok.Payload.ContainsKey("missing"));
        }
    }
}