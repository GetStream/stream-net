using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stream;
using System.Threading;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class IntegrationTests
    {
        private const int SetupDelay = 3000;
        private const int AddDelay = 7000;
        private const int UpdateDelay = 7000;
        private const int RemoveDelay = 5000;
        private const int FollowDelay = 5000;
        private const int MarkDelay = 5000;


        private Stream.StreamClient _client;
        private Stream.StreamFeed _user1;
        private Stream.StreamFeed _user2;
        private Stream.StreamFeed _flat3;
        private Stream.StreamFeed _agg4;
        private Stream.StreamFeed _not5;

        [SetUp]
        public void Setup()
        {
            _client = new Stream.StreamClient(
                "ea7xzzkj6kc4",
                "zd8cdv9rhxcpmkx9zx4jqt7q9qhawpgsfpay2gy7jaubym32crs9kaux2pm67wrx",
                new Stream.StreamClientOptions()
                {
                    Location = Stream.StreamApiLocation.USWest
                });
            _user1 = _client.Feed("user", "11");
            _user2 = _client.Feed("user", "22");
            _flat3 = _client.Feed("flat", "333");
            _agg4 = _client.Feed("aggregate", "444");
            _not5 = _client.Feed("notification", "555");

            _user1.Delete().GetAwaiter().GetResult();
            _user2.Delete().GetAwaiter().GetResult();
            _flat3.Delete().GetAwaiter().GetResult();
            _agg4.Delete().GetAwaiter().GetResult();
            _not5.Delete().GetAwaiter().GetResult();
            System.Threading.Thread.Sleep(SetupDelay);
        }

        [Test]
        public async Task TestAddActivity()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

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
            var newActivity = new Stream.Activity("1", "test", "1")
            {
                Time = now
            };
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

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

            Thread.Sleep(AddDelay);

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
            int second = 2;
            double third = 3;
            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData("complex", "string");
            newActivity.SetData("second", second);
            newActivity.SetData("third", third);
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

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

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<IDictionary<String, String>>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual(2, complex.Count);
            Assert.AreEqual("shawn", complex["test1"]);
        }

        public class CustomClass
        {
            public string TestString { get; set; }

            public int TestInt { get; set; }

            public double TestDouble { get; set; }
        }

        [Test]
        public async Task TestAddActivityWithDifferentVariables()
        {
            int second = 2;
            double third = 3;
            var dict = new Dictionary<string, object>();
            dict["test1"] = "shawn";
            dict["test2"] = "wedge";
            dict["test3"] = 42;

            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData("complex", dict);
            newActivity.SetData("second", second);
            newActivity.SetData("third", third);
            newActivity.SetData("customc", new CustomClass()
            {
                TestString = "string",
                TestInt = 123,
                TestDouble = 42.2
            });

            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<IDictionary<string, object>>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual(3, complex.Count);
            Assert.AreEqual("shawn", complex["test1"]);
            Assert.AreEqual("wedge", complex["test2"]);
            Assert.AreEqual(42, complex["test3"]);

            Assert.AreEqual(2, first.GetData<int>("second"));

            Assert.AreEqual(3M, first.GetData<double>("third"));

            var customc = first.GetData<CustomClass>("customc");
            Assert.IsNotNull(customc);
            Assert.AreEqual("string", customc.TestString);
            Assert.AreEqual(123, customc.TestInt);
            Assert.AreEqual(42.2, customc.TestDouble);
        }

        [Test]
        public async Task TestAddActivityVariablesOldWayNewWay()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData<string>("stringint", "42");
            newActivity.SetData<string>("stringdouble", "42.2");
            newActivity.SetData<string>("stringcomplex", "{ \"test1\": 1, \"test2\": \"testing\" }");

            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);
            
            Assert.AreEqual(42, first.GetData<int>("stringint"));

            Assert.AreEqual(42.2, first.GetData<double>("stringdouble"));

            var complex = first.GetData<IDictionary<string, object>>("stringcomplex");
            Assert.IsNotNull(complex);
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

            Thread.Sleep(AddDelay);

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

            Thread.Sleep(AddDelay * 2);

            var activities = await this._user1.GetActivities(0, 2);
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            Assert.AreEqual(response.Skip(1).First().Id, activities.First().Id);
            Assert.AreEqual(response.First().Id, activities.Skip(1).First().Id);
        }

        [Test]
        public async Task TestUpdateActivity()
        {
            var newActivity = new Stream.Activity("1", "test", "1")
            {
                ForeignId = "post:1",
                Time = DateTime.UtcNow
            };
            newActivity.SetData<string>("myData", "1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            first.Actor = "editedActor1";
            first.Object = "editedOject1";
            first.Verb = "editedVerbTest";
            first.SetData<string>("myData", "editedMyData1");

            await this._user1.UpdateActivity(first);

            Thread.Sleep(UpdateDelay);

            activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var editedFirst = activities.First();
            Assert.AreEqual(first.Id, editedFirst.Id);
            Assert.AreEqual(first.GetData<string>("myData"), editedFirst.GetData<string>("myData"));
            Assert.AreEqual(first.Actor, editedFirst.Actor);
            Assert.AreEqual(first.Object, editedFirst.Object);
            Assert.AreEqual(first.Verb, editedFirst.Verb);
        }
        
        [Test]
        public async Task TestUpdateActivities()
        {
            var newActivities = new Stream.Activity[] {
                new Stream.Activity("multi1", "test", "1")
                {
                    ForeignId = "post:1",
                    Time = DateTime.UtcNow
                },
                new Stream.Activity("multi2", "test", "2")
                {
                    ForeignId = "post:2",
                    Time = DateTime.UtcNow
                }
            };

            var response = await this._user1.AddActivities(newActivities);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            Thread.Sleep(AddDelay * 2);

            var activities = (await this._user1.GetActivities(0, 2)).ToArray();
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            Assert.AreEqual(response.First().Id, activities.First().Id);
            Assert.AreEqual(response.Skip(1).First().Id, activities.Skip(1).First().Id);

            for (int i = 0; i < activities.Length; i++)
            {
                activities[i].Actor = "editedActor" + activities[i].Actor;
                activities[i].Object = "editedObject" + activities[i].Object;
                activities[i].Verb = "editedVerb" + activities[i].Verb;
            }

            await this._user1.UpdateActivities(activities);

            Thread.Sleep(UpdateDelay);

            var editedActivities = (await this._user1.GetActivities(0, 2)).ToArray();
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            for (int i = 0; i < activities.Length; i++)
            {
                Assert.AreEqual(activities[i].Id, editedActivities[i].Id);
                Assert.AreEqual(activities[i].Actor, editedActivities[i].Actor);
                Assert.AreEqual(activities[i].Object, editedActivities[i].Object);
                Assert.AreEqual(activities[i].Verb, editedActivities[i].Verb);
            }
        }

        [Test]
        public async Task TestRemoveActivity()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.FirstOrDefault();
            Assert.AreEqual(response.Id, first.Id);

            await this._user1.RemoveActivity(first.Id);

            Thread.Sleep(RemoveDelay);

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

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual(fid, activities.First().ForeignId);
            
            await this._user1.RemoveActivity(fid, true);

            Thread.Sleep(RemoveDelay);

            activities = await this._user1.GetActivities(0, 1);
            Assert.AreEqual(0, activities.Count());
        }

        [Test]
        public async Task TestDelete()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._user1.AddActivity(newActivity);
            Assert.IsNotNull(response);

            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            await this._user1.Delete();

            Thread.Sleep(RemoveDelay);

            var nextActivities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(nextActivities);
            Assert.AreEqual(0, nextActivities.Count());
        }

        [Test]
        public async Task TestGet()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            var first = await this._user1.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "test", "2");
            var second = await this._user1.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "test", "3");
            var third = await this._user1.AddActivity(newActivity);

            Thread.Sleep(AddDelay * 3);

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
        public async Task TestGetFlatActivities()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            var first = await this._user1.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "test", "2");
            var second = await this._user1.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "test", "3");
            var third = await this._user1.AddActivity(newActivity);

            Thread.Sleep(AddDelay * 3);

            var response = await this._user1.GetFlatActivities(GetOptions.Default.WithLimit(2));
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Duration);
            var activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());
            Assert.AreEqual(third.Id, activities.First().Id);
            Assert.AreEqual(second.Id, activities.Skip(1).First().Id);

            response = await this._user1.GetFlatActivities(GetOptions.Default.WithOffset(1).WithLimit(2));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            response = await this._user1.GetFlatActivities(GetOptions.Default.WithLimit(2).WithFilter(FeedFilter.Where().IdLessThan(third.Id)));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed("flat", "333", false);
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed("flat", "333");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeed("flat", "333");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryFalse()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed("flat", "333", false);
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed("flat", "333");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeed("flat", "333", false);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryTrue()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed("flat", "333", false);
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed("flat", "333");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeed("flat", "333", true);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowByFeedKeepHistoryDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(_flat3, false);
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed(_flat3);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeed(_flat3);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowUnfollowByFeedKeepHistoryFalse()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(_flat3, false);
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed(_flat3);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeed(_flat3, false);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowUnfollowByFeedKeepHistoryTrue()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(_flat3, false);
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed(_flat3);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeed(_flat3, true);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryDefault()
        {
            var secret = this._client.Feed("secret", "33");

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed("secret", "33");
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await secret.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed("secret", "33");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeed("secret", "33");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryFalse()
        {
            var secret = this._client.Feed("secret", "33");

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed("secret", "33");
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await secret.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed("secret", "33");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeed("secret", "33", false);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(0, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryTrue()
        {
            var secret = this._client.Feed("secret", "33");

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed("secret", "33");
            Thread.Sleep(FollowDelay);
            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await secret.AddActivity(newActivity);

                Thread.Sleep(AddDelay);

                await this._user1.FollowFeed("secret", "33");
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeed("secret", "33", true);
                Thread.Sleep(FollowDelay);

                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestMarkRead()
        {
            var newActivity = new Stream.Activity("1", "tweet", "1");
            var first = await _not5.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "run", "2");
            var second = await _not5.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "share", "3");
            var third = await _not5.AddActivity(newActivity);

            Thread.Sleep(AddDelay * 3);

            var activities = await _not5.GetActivities(0, 2, marker: ActivityMarker.Mark().AllRead());
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var notActivity = activities.First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            Thread.Sleep(MarkDelay);

            activities = await _not5.GetActivities(0, 2);
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            notActivity = activities.First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);
        }

        [Test]
        public async Task TestMarkReadByIds()
        {
            var newActivity = new Stream.Activity("1", "tweet", "1");
            var first = await _not5.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "run", "2");
            var second = await _not5.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "share", "3");
            var third = await _not5.AddActivity(newActivity);

            Thread.Sleep(AddDelay * 3);

            var activities = await _not5.GetActivities(0, 2);

            var marker = ActivityMarker.Mark();
            foreach (var activity in activities)
            {
                marker = marker.Read(activity.Id);
            }

            var notActivity = activities.First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            activities = await _not5.GetActivities(0, 3, marker: marker);

            Thread.Sleep(MarkDelay);

            activities = await _not5.GetActivities(0, 3);
            Assert.IsNotNull(activities);
            Assert.AreEqual(3, activities.Count());

            notActivity = activities.First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(2).First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);
        }

        [Test]
        public async Task TestMarkNotificationsRead()
        {
            var newActivity = new Stream.Activity("1", "tweet", "1");
            var first = await _not5.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "run", "2");
            var second = await _not5.AddActivity(newActivity);

            newActivity = new Stream.Activity("1", "share", "3");
            var third = await _not5.AddActivity(newActivity);

            Thread.Sleep(AddDelay * 3);

            var response = await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(2).WithMarker(ActivityMarker.Mark().AllRead()));
            Assert.IsNotNull(response);

            var activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            Thread.Sleep(MarkDelay);

            response = await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(2));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Unread);            

            activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);
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
            var feed = this._client.Feed("flat", "csharp42");
            var feed1 = this._client.Feed("flat", "csharp43");
            var feed2 = this._client.Feed("flat", "csharp44");

            // unfollow
            await feed1.UnfollowFeed(feed);
            await feed2.UnfollowFeed(feed);

            Thread.Sleep(FollowDelay * 2);

            var response = await feed.Followers(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeed(feed);
            await feed2.FollowFeed(feed);

            Thread.Sleep(FollowDelay * 2);

            response = await feed.Followers(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var first = response.First();
            Assert.AreEqual(first.FeedId, "flat:csharp44");
            Assert.AreEqual(first.TargetId, "flat:csharp42");

            Assert.IsTrue(first.CreatedAt > DateTime.Now.AddDays(-1));
            Assert.IsTrue(first.UpdatedAt > DateTime.Now.AddDays(-1));
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
            var feed = this._client.Feed("flat", "csharp42");
            var feed1 = this._client.Feed("flat", "csharp43");
            var feed2 = this._client.Feed("flat", "csharp44");

            // unfollow
            await feed1.UnfollowFeed(feed);
            await feed1.UnfollowFeed(feed2);

            Thread.Sleep(FollowDelay * 2);

            var response = await feed1.Following(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeed(feed);
            await feed1.FollowFeed(feed2);

            Thread.Sleep(FollowDelay * 2);

            response = await feed1.Following(0, 2);
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

        [Test]
        public async Task TestAggregate()
        {
            var newActivity1 = new Stream.Activity("1", "test", "1");
            var newActivity2 = new Stream.Activity("1", "test", "2");
            var response = await _user1.AddActivity(newActivity1);
            response = await _user1.AddActivity(newActivity2);

            Thread.Sleep(AddDelay * 2);

            await _agg4.FollowFeed("user", "11");
            Thread.Sleep(FollowDelay);

            var activities = await this._agg4.GetActivities(0);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(2, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await _agg4.UnfollowFeed("user", "11");
        }

        [Test]
        public async Task TestGetAggregate()
        {
            var newActivity1 = new Stream.Activity("1", "test", "1");
            var newActivity2 = new Stream.Activity("1", "test", "2");
            var response = await _user1.AddActivity(newActivity1);
            response = await _user1.AddActivity(newActivity2);

            Thread.Sleep(AddDelay * 2);

            await _agg4.FollowFeed("user", "11");
            Thread.Sleep(FollowDelay);

            var result = await this._agg4.GetAggregateActivities();
            var activities = result.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var aggActivity = activities.First();
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(2, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);
            Assert.IsNotNull(aggActivity.CreatedAt);
            Assert.IsTrue(Math.Abs(aggActivity.CreatedAt.Value.Subtract(DateTime.UtcNow).TotalMinutes) < 10);
            Assert.IsNotNull(aggActivity.UpdatedAt);
            Assert.IsTrue(Math.Abs(aggActivity.UpdatedAt.Value.Subtract(DateTime.UtcNow).TotalMinutes) < 10);
            Assert.IsNotNull(aggActivity.Group);

            await _agg4.UnfollowFeed("user", "11");
        }

        [Test]
        public async Task TestMixedAggregate()
        {
            var newActivity1 = new Stream.Activity("1", "test", "1");
            var newActivity2 = new Stream.Activity("1", "test", "2");
            var newActivity3 = new Stream.Activity("1", "other", "2");
            var response = await _user1.AddActivity(newActivity1);
            response = await _user1.AddActivity(newActivity2);
            response = await _user1.AddActivity(newActivity3);

            Thread.Sleep(AddDelay * 3);

            await _agg4.FollowFeed("user", "11");
            Thread.Sleep(FollowDelay);

            var activities = await this._agg4.GetActivities(0);
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(1, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await _agg4.UnfollowFeed("user", "11");
        }


        [Test]
        public async Task TestBatchFollow()
        {
            await _client.Batch.FollowMany(new[]
            {
                new Follow(_user1, _flat3),
                new Follow(_user2, _flat3)
            });
            Thread.Sleep(FollowDelay * 2);

            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._flat3.AddActivity(newActivity);
            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);

            activities = await this._user2.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);
        }

        [Test]
        public async Task TestBatchFollowWithCopyLimit()
        {
            await _client.Batch.FollowMany(new[]
            {
                new Follow("user:11", "flat:333"),
                new Follow("user:22", "flat:333")
            }, 10);
            Thread.Sleep(FollowDelay * 2);

            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._flat3.AddActivity(newActivity);
            Thread.Sleep(AddDelay);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);

            activities = await this._user2.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);
        }


        [Test]
        public async Task TestAddToMany()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
            await _client.Batch.AddToMany(newActivity, new[]
            {
                _user1, _user2
            });
            Thread.Sleep(AddDelay * 2);

            var activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(newActivity.Actor, first.Actor);
            Assert.AreEqual(newActivity.Object, first.Object);
            Assert.AreEqual(newActivity.Verb, first.Verb);

            activities = await this._user2.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            first = activities.First();
            Assert.AreEqual(newActivity.Actor, first.Actor);
            Assert.AreEqual(newActivity.Object, first.Object);
            Assert.AreEqual(newActivity.Verb, first.Verb);
        }
    }
}
