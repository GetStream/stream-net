using NUnit.Framework;

namespace StreamNetTests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class CollectionsTest
    {
        [Test]
        public void TestRefWithCollection()
        {
            var collectionObj = new Stream.CollectionObject("id-col");
            var refString = Stream.Collections.Ref("col", collectionObj);
            Assert.AreEqual("SO:col:id-col", refString);
        }

        [Test]
        public void TestRefWithCollectionObjectID()
        {
            var refString = Stream.Collections.Ref("col", "id-col");
            Assert.AreEqual("SO:col:id-col", refString);
        }
    }
}
