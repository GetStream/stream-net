using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stream;

namespace stream_net_tests
{
    [TestFixture]
    public class AsiaJapanIntegrationTests
    {
        private Stream.StreamClient _client;
        private Stream.StreamFeed _user1;
        private Stream.StreamFeed _flat3;

        [SetUp]
        public void Setup()
        {
            _client = new Stream.StreamClient(
                "hcppd32schhy",
                "8e9smyj47fbkm2eezq8fxhr2bw45j8x88kqx3pzb9z4jbnxfs99tttkzsus8d8ym",
                new Stream.StreamClientOptions()
                {
                    Location = Stream.StreamApiLocation.AsiaJapan
                });
            _user1 = _client.Feed("user", "11");
            _flat3 = _client.Feed("flat", "333");

            //System.Threading.Thread.Sleep(3000);
        }

        [Test]
        public async Task TestAddActivity()
        {
            var newActivity = new Stream.Activity("1","test","1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);
            Assert.AreEqual(response.Actor, first.Actor);
            Assert.AreEqual(response.Object, first.Object);
            Assert.AreEqual(response.Verb, first.Verb);
        }

        [Test]
        public async Task TestAddActivityWithTime()
        {
            var now = DateTime.UtcNow;
            var newActivity = new Stream.Activity("1","test","1")
            {
                Time = now
            };
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            // using long date string here to skip milliseconds
            //  "now" will have the milliseconds whereas the response or lookup wont
            Assert.AreEqual(now.ToLongDateString(), first.Time.Value.ToLongDateString());
        }

        [Test]
        public async Task TestAddActivityWithArray()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData("complex", new String[] { "tommaso", "thierry", "shawn" });
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<String[]>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual(3, complex.Length);
            Assert.AreEqual("shawn", complex[2]);
        }

        [Test]
        public async Task TestAddActivityWithString()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData("complex", "string");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<String>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual("string", complex);
        }

        [Test]
        public async Task TestAddActivityWithDictionary()
        {
            var dict = new Dictionary<String, String>();
            dict["test1"] = "shawn";
            dict["test2"] = "wedge";

            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData("complex", dict);
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<IDictionary<String,String>>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual(2, complex.Count);
            Assert.AreEqual("shawn", complex["test1"]);
        }

        [Test]
        public async Task TestAddActivityTo()
        {
            var newActivity = new Stream.Activity("multi1", "test", "1")
            {
                To = new List<String>("flat:remotefeed1".Yield())
            };
            var addedActivity = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(addedActivity);
            Assert.IsNotNull(addedActivity.To);
            Assert.AreEqual(1, addedActivity.To.SafeCount());
            Assert.AreEqual("flat:remotefeed1", addedActivity.To.First());

            var activities = await _client.Feed("flat", "remotefeed1").GetActivities(0, 1);            
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();            
            Assert.AreEqual("multi1", first.Actor);
        }    

        [Test]
        public async Task TestAddActivities()
        {
            var newActivities = new Stream.Activity[] {
                new Stream.Activity("multi1","test","1"),
                new Stream.Activity("multi2","test","2")
            };
            
            var response = await this._user1.AddActivities(newActivities);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var activities = await this._user1.GetActivities(0, 2);
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            Assert.AreEqual(response.Skip(1).First().Id, activities.First().Id);
            Assert.AreEqual(response.First().Id, activities.Skip(1).First().Id);
        }

        [Test]
        public async Task TestRemoveActivity()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.FirstOrDefault();
            Assert.AreEqual(response.Id, first.Id);

            await this._user1.RemoveActivity(first.Id);
            var nextActivities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(nextActivities);
            Assert.IsFalse(nextActivities.Any(na => na.Id == first.Id));
        }

        [Test]
        public async Task TestRemoveActivityByForeignId()
        {
            var fid = "post:42";
            var newActivity = new Stream.Activity("1", "test", "1")
            {
                ForeignId = fid
            };

            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);
        
            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual(fid, activities.First().ForeignId);        
        
            await this._user1.RemoveActivity(fid, true);
            activities = await this._user1.GetActivities(0, 1);
            Assert.AreEqual(1, activities.Count());
            Assert.AreNotEqual(response.Id, activities.First().Id);
        }

        [Test]
        public async Task TestDelete() 
        { 
            var newActivity = new Stream.Activity("1","test","1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            await this._user1.Delete();

            var nextActivities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(nextActivities);
            Assert.AreEqual(0, nextActivities.Count());
        } 

        [Test]
        public async Task TestGet()
        {
            var newActivity = new Stream.Activity("1","test","1");
            var first = await this._user1.AddActivity(newActivity);

            newActivity = new Stream.Activity("1","test","2");
            var second = await this._user1.AddActivity(newActivity);

            newActivity = new Stream.Activity("1","test","3");
            var third = await this._user1.AddActivity(newActivity);

            var activities = await this._user1.GetActivities(0, 2);
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());
            Assert.AreEqual(third.Id, activities.First().Id);
            Assert.AreEqual(second.Id, activities.Skip(1).First().Id);

            activities = await this._user1.GetActivities(1, 2);
            Assert.AreEqual(second.Id, activities.First().Id);

            //$id_offset =  ['id_lt' => $third_id];
            activities = await this._user1.GetActivities(0, 2, FeedFilter.Where().IdLessThan(third.Id));
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestFlatFollowUnfollow()
        {
            this._user1.UnfollowFeed("flat", "333").Wait();
            System.Threading.Thread.Sleep(3000);

            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._flat3.AddActivity(newActivity);

            this._user1.FollowFeed("flat", "333").Wait();
            System.Threading.Thread.Sleep(5000);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);

            this._user1.UnfollowFeed("flat", "333").Wait();
            System.Threading.Thread.Sleep(3000);

            activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreNotEqual(response.Id, activities.First().Id);
        }


        [Test]
        public async Task TestFlatFollowUnfollowPrivate()
        {
            var secret = this._client.Feed("secret", "33");

            this._user1.UnfollowFeed("secret", "33").Wait();
            System.Threading.Thread.Sleep(3000);

            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await secret.AddActivity(newActivity);

            this._user1.FollowFeed("secret", "33").Wait();
            System.Threading.Thread.Sleep(5000);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);

            await this._user1.UnfollowFeed("secret", "33");
        }

        [Test]
        public async Task TestFollowersEmpty()
        {
            var lonely = this._client.Feed("flat", "lonely");
            var response = await lonely.Followers();
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowersWithLimit()
        {
            this._client.Feed("flat", "csharp43").FollowFeed("flat", "csharp42").Wait();
            this._client.Feed("flat", "csharp44").FollowFeed("flat", "csharp42").Wait();
            var response = await this._client.Feed("flat", "csharp42").Followers(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());
            Assert.AreEqual(response.First().FeedId, "flat:csharp44");
            Assert.AreEqual(response.First().TargetId, "flat:csharp42");
        }

        [Test]
        public async Task TestFollowingEmpty()
        {
            var lonely = this._client.Feed("flat", "lonely");
            var response = await lonely.Following();
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowingsWithLimit()
        {
            this._client.Feed("flat", "csharp43").FollowFeed("flat", "csharp42").Wait();
            this._client.Feed("flat", "csharp43").FollowFeed("flat", "csharp44").Wait();
            var response = await this._client.Feed("flat", "csharp43").Following(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());
            Assert.AreEqual(response.First().FeedId, "flat:csharp43");
            Assert.AreEqual(response.First().TargetId, "flat:csharp44");
        }

        [Test]
        public async Task TestDoIFollowEmpty()
        {
            var lonely = this._client.Feed("flat", "lonely");
            var response = await lonely.Following(0, 10, new String[] { "flat:asocial" });
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }
    }
}
