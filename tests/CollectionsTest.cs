using NUnit.Framework;
using Stream;
using Stream.Models;

namespace StreamNetTests
{
    [TestFixture]
    public class CollectionsTest
    {
        private ICollections _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = Credentials.Instance.Client.Collections;
        }

        [Test]
        public void TestRefWithCollection()
        {
            var collectionObj = new CollectionObject("id-col");
            var refString = _client.Ref("col", collectionObj);
            Assert.AreEqual("SO:col:id-col", refString);
        }

        [Test]
        public void TestRefWithCollectionObjectID()
        {
            var refString = _client.Ref("col", "id-col");
            Assert.AreEqual("SO:col:id-col", refString);
        }
    }
}
