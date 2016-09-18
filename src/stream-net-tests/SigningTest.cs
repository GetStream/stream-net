using System;
using NUnit.Framework;
using Stream;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class SigningTest
    {
        [Test]
        public void TokenIsValid()
        {
            var client = new StreamClient("key", "gthc2t9gh7pzq52f6cky8w4r4up9dr6rju9w3fjgmkv6cdvvav2ufe5fv7e2r9qy");

            var feed = client.Feed("flat", "1");

            Assert.AreEqual("iFX1l5f_lIUWgZFBnv5UisTTW18", feed.Token);
        }

        [Test]
        public void ReadOnlyTokenIsValid()
        {
            var client = new StreamClient("key", "gthc2t9gh7pzq52f6cky8w4r4up9dr6rju9w3fjgmkv6cdvvav2ufe5fv7e2r9qy");

            var feed = client.Feed("flat", "1");
            var token = feed.ReadOnlyToken;
            Assert.AreEqual("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJyZXNvdXJjZSI6IioiLCJhY3Rpb24iOiIqIiwiZmVlZF9pZCI6ImZsYXQxIn0.7435I3bhISLU2RdVeVVMtmjhLE7LPHvDgqQ6mnfFwhU", token);
        }
    }
}
