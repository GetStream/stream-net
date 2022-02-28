using NUnit.Framework;
using Stream;
using Stream.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class CollectionTests : TestBase
    {
        [Test]
        public void TestRefWithCollection()
        {
            var collectionObj = new CollectionObject("id-col");
            var refString = Client.Collections.Ref("col", collectionObj);
            Assert.AreEqual("SO:col:id-col", refString);
        }

        [Test]
        public void TestRefWithCollectionObjectID()
        {
            var refString = Client.Collections.Ref("col", "id-col");
            Assert.AreEqual("SO:col:id-col", refString);
        }

        [Test]
        public async Task TestCollectionsCRUD()
        {
            var colData = new Dictionary<string, object>();
            colData.Add("field", "value");
            colData.Add("flag", true);

            // ADD
            var collectionObject = await Client.Collections.AddAsync("col_test_crud", colData);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.Id));
            Assert.AreEqual("value", collectionObject.GetData<string>("field"));
            Assert.AreEqual(true, collectionObject.GetData<bool>("flag"));

            Assert.ThrowsAsync<StreamException>(async () =>
            {
                var o = await Client.Collections.AddAsync("col_test_crud", colData, collectionObject.Id);
            });

            // GET
            collectionObject = await Client.Collections.GetAsync("col_test_crud", collectionObject.Id);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.Id));
            Assert.AreEqual("value", collectionObject.GetData<string>("field"));
            Assert.AreEqual(true, collectionObject.GetData<bool>("flag"));

            // UPDATE
            var newData = new Dictionary<string, object>();
            newData.Add("new", "stuff");
            newData.Add("arr", new[] { "a", "b" });
            collectionObject = await Client.Collections.UpdateAsync("col_test_crud", collectionObject.Id, newData);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.Id));
            Assert.AreEqual("stuff", collectionObject.GetData<string>("new"));
            Assert.AreEqual(new[] { "a", "b" }, collectionObject.GetData<string[]>("arr"));

            // DELETE
            await Client.Collections.DeleteAsync("col_test_crud", collectionObject.Id);

            Assert.ThrowsAsync<StreamException>(async () =>
            {
                var o = await Client.Collections.GetAsync("col_test_crud", collectionObject.Id);
            });
        }

        [Test]
        public async Task TestCollectionsDelete()
        {
            string id1 = System.Guid.NewGuid().ToString(),
            id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await Client.Collections.UpsertManyAsync("people", data);

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Collections.DeleteAsync("people", id2);
            });

            var results = (await Client.Collections.SelectManyAsync("people", new[] { id1, id2 })).Response.Data;

            Assert.NotNull(results);
            Assert.AreEqual(1, results.CountOrFallback());
            var result = results.First();
            Assert.AreEqual(id1, result.Id);
            Assert.AreEqual(data1.GetData<List<string>>("hobbies"), result.Data.GetData<List<string>>("hobbies"));
        }

        [Test]
        public async Task TestCollectionsDeleteMany()
        {
            string id1 = System.Guid.NewGuid().ToString(),
            id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await Client.Collections.UpsertManyAsync("people", data);

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Collections.DeleteManyAsync("people", new[] { id1, id2 });
            });

            var results = (await Client.Collections.SelectManyAsync("people", new[] { id1, id2 })).Response.Data;

            Assert.NotNull(results);
            Assert.AreEqual(0, results.CountOrFallback());
        }

        [Test]
        public async Task TestCollectionsSelect()
        {
            string id1 = System.Guid.NewGuid().ToString(),
            id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            data1.SetData(new Dictionary<string, object> { ["name"] = "John" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await Client.Collections.UpsertManyAsync("people", data);

            var result = await Client.Collections.SelectAsync("people", id1);

            Assert.NotNull(result);
            Assert.AreEqual(data1.Id, result.Id);
            Assert.AreEqual(data1.GetData<List<string>>("hobbies"), result.Data.GetData<List<string>>("hobbies"));
            Assert.AreEqual(data1.GetData<string>("name"), result.Data.GetData<string>("name"));
        }

        [Test]
        public async Task TestCollectionsSelectMany()
        {
            var id1 = System.Guid.NewGuid().ToString();
            var id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await Client.Collections.UpsertManyAsync("people", data);

            var results = (await Client.Collections.SelectManyAsync("people", new[] { id1, id2 })).Response.Data;

            Assert.NotNull(results);
            Assert.AreEqual(data.Count, results.CountOrFallback());
            results.ForEach(r =>
            {
                var found = data.First(x => x.Id == r.Id);
                var key = r.Id.Equals(id1) ? "hobbies" : "vacation";
                Assert.AreEqual(found.GetData<List<string>>(key), r.Data.GetData<List<string>>(key));
            });
        }

        [Test]
        public void TestCollectionsUpsert()
        {
            var data = new CollectionObject(System.Guid.NewGuid().ToString());
            data.SetData("hobbies", new List<string> { "eating", "coding" });

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Collections.UpsertAsync("people", data);
            });
        }

        [Test]
        public void TestCollectionsUpsertMany()
        {
            var data1 = new CollectionObject(System.Guid.NewGuid().ToString());
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(System.Guid.NewGuid().ToString());
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            Assert.DoesNotThrowAsync(async () =>
            {
                await Client.Collections.UpsertManyAsync("people", data);
            });
        }
    }
}