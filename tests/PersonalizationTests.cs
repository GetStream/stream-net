using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class PersonalizationTests : TestBase
    {
        [Test]
        [Ignore("Not always needed, set credentials to run when needed")]
        public async Task ReadPersonalization()
        {
            var response = await Client.Personalization.GetAsync("etoro_newsfeed", new Dictionary<string, object>
            {
                { "feed_slug", "newsfeed" },
                { "user_id", "crembo" },
                { "limit", 20 },
                { "ranking", "etoro" },
            });

            var d = new Dictionary<string, object>(response);
            Assert.AreEqual(41021, d["app_id"]);
            Assert.True(d.ContainsKey("duration"));
            Assert.True(d.ContainsKey("results"));
        }
    }
}