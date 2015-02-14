using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stream;

namespace stream_net_tests
{
    [TestClass]
    public class SigningTest
    {
        [TestMethod]
        public void TokenIsValid()
        {
            var client = new StreamClient("key", "gthc2t9gh7pzq52f6cky8w4r4up9dr6rju9w3fjgmkv6cdvvav2ufe5fv7e2r9qy");

            var feed = client.Feed("flat", "1");

            Assert.AreEqual("iFX1l5f_lIUWgZFBnv5UisTTW18", feed.Token);
        }
    }
}
