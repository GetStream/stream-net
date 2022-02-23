using NUnit.Framework;
using Stream.Models;
using System.IO;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class FileTests : TestBase
    {
        [Test]
        public async Task TestUpload()
        {
            Upload upload;

            using (var fs = File.OpenRead("../../../helloworld.txt"))
            {
                upload = await Client.Files.UploadAsync(fs, "helloworld.txt");
                Assert.IsNotEmpty(upload.File);

                await Client.Files.DeleteAsync(upload.File);
            }

            using (var fs = File.OpenRead("../../../helloworld.txt"))
            {
                upload = await Client.Files.UploadAsync(fs, "helloworld.txt", "text/plain");
                Assert.IsNotEmpty(upload.File);

                await Client.Files.DeleteAsync(upload.File);
            }

            using (var fs = File.OpenRead(@"../../../helloworld.jpg"))
            {
                upload = await Client.Images.UploadAsync(fs, "helloworld.jpg", "image/jpeg");
                Assert.IsNotEmpty(upload.File);

                await Client.Images.DeleteAsync(upload.File);
            }
        }
    }
}