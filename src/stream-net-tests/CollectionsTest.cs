using NUnit.Framework;
using System;
using GetStream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class CollectionsTest
    {
        [Test]
        public void TestRefWithCollection()
        {
            var collectionObj = new GetStream.CollectionObject("id-col");
            var refString = GetStream.Collections.Ref("col", collectionObj);
            Assert.AreEqual("SO:col:id-col", refString);
        }

        [Test]
        public void TestRefWithCollectionObjectID()
        {
            var refString = GetStream.Collections.Ref("col", "id-col");
            Assert.AreEqual("SO:col:id-col", refString);
        }
    }
}
