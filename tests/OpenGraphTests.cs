using NUnit.Framework;
using Stream;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class OpenGraphTests : TestBase
    {
        [Test]
        public async Task TestOG()
        {
            var og = await Client.OgAsync("https://getstream.io/blog/try-out-the-stream-api-with-postman");

            Assert.IsNotEmpty(og.Type);
            Assert.IsNotEmpty(og.Title);
            Assert.IsNotEmpty(og.Description);
            Assert.IsNotEmpty(og.Url);
            Assert.IsNotEmpty(og.Favicon);
            Assert.IsNotEmpty(og.Images);
            Assert.IsNotEmpty(og.Images[0].Image);
        }
    }
}