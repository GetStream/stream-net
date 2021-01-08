using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stream;
using Newtonsoft.Json;
using System.Threading;

namespace stream_net_tests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class IntegrationTests
    {
        private Stream.IStreamClient _client;
        private Stream.IStreamFeed _user1;
        private Stream.IStreamFeed _user2;
        private Stream.IStreamFeed _flat3;
        private Stream.IStreamFeed _agg4;
        private Stream.IStreamFeed _not5;

        [SetUp]
        public void Setup()
        {
            _client = new Stream.StreamClient(
                "ea7xzzkj6kc4",
                "zd8cdv9rhxcpmkx9zx4jqt7q9qhawpgsfpay2gy7jaubym32crs9kaux2pm67wrx",
                new Stream.StreamClientOptions()
                {
                    Location = Stream.StreamApiLocation.USEast,
                    Timeout = 10000
                });
            _user1 = _client.Feed("user", System.Guid.NewGuid().ToString());
            _user2 = _client.Feed("user", System.Guid.NewGuid().ToString());
            _flat3 = _client.Feed("flat", System.Guid.NewGuid().ToString());
            _agg4 = _client.Feed("aggregate", System.Guid.NewGuid().ToString());
            _not5 = _client.Feed("notification", System.Guid.NewGuid().ToString());
        }

        [Test]
        public async Task TestAddActivity()
        {
            var newActivity = new Stream.Activity("1", "test", "1");
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
            var newActivity = new Stream.Activity("1", "test", "1")
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
            Assert.AreEqual(now.ToString(), first.Time.Value.ToString());
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
            int second = 2;
            double third = 3;
            var newActivity = new Stream.Activity("1", "test", "1");
            newActivity.SetData("complex", "string");
            newActivity.SetData("second", second);
            newActivity.SetData("third", third);
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
            Assert.AreEqual(0, activities.Count());
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

            response = await this._user1.GetFlatActivities(GetOptions.Default.WithLimit(2).WithSession("dummy").WithFilter(FeedFilter.Where().IdLessThan(third.Id)));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(this._flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);



                await this._user1.FollowFeed(this._flat3);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeed(this._flat3);


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
            await this._user1.UnfollowFeed(this._flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);



                await this._user1.FollowFeed(this._flat3);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeed(this._flat3, false);


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
            await this._user1.UnfollowFeed(this._flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._flat3.AddActivity(newActivity);

            await this._user1.FollowFeed(this._flat3);

            activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);

            //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
            await this._user1.UnfollowFeed(this._flat3, true);


            activities = await this._user1.GetActivities(0, 1);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
        }
        [Test]
        public async Task TestFlatFollowActivityCopyLimitDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(this._flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Stream.Activity("1", "test", "1"), new Stream.Activity("1", "test", "2"), new Stream.Activity("1", "test", "3"), new Stream.Activity("1", "test", "4"), new Stream.Activity("1", "test", "5") };
                var response = await this._flat3.AddActivities(newActivities);



                await this._user1.FollowFeed(this._flat3);


                activities = await this._user1.GetActivities(0, 5);
                Assert.IsNotNull(activities);
                Assert.AreEqual(5, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowActivityCopyLimitNonDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(this._flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Stream.Activity("1", "test", "1"), new Stream.Activity("1", "test", "2"), new Stream.Activity("1", "test", "3"), new Stream.Activity("1", "test", "4"), new Stream.Activity("1", "test", "5") };
                var response = await this._flat3.AddActivities(newActivities);



                await this._user1.FollowFeed(this._flat3, 3);


                activities = await this._user1.GetActivities(0, 5);
                Assert.IsNotNull(activities);
                Assert.AreEqual(3, activities.Count());
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

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);



                await this._user1.FollowFeed(_flat3);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeed(_flat3);


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

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);



                await this._user1.FollowFeed(_flat3);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeed(_flat3, false);


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

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await this._flat3.AddActivity(newActivity);



                await this._user1.FollowFeed(_flat3);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeed(_flat3, true);


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
        public async Task TestFlatFollowByFeedActivityCopyLimitDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(_flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Stream.Activity("1", "test", "1"), new Stream.Activity("1", "test", "2"), new Stream.Activity("1", "test", "3"), new Stream.Activity("1", "test", "4"), new Stream.Activity("1", "test", "5") };
                var response = await this._flat3.AddActivities(newActivities);



                await this._user1.FollowFeed(_flat3);


                activities = await this._user1.GetActivities(0, 5);
                Assert.IsNotNull(activities);
                Assert.AreEqual(5, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowByFeedActivityCopyLimitNonDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(_flat3, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Stream.Activity("1", "test", "1"), new Stream.Activity("1", "test", "2"), new Stream.Activity("1", "test", "3"), new Stream.Activity("1", "test", "4"), new Stream.Activity("1", "test", "5") };
                var response = await this._flat3.AddActivities(newActivities);



                await this._user1.FollowFeed(_flat3, 3);


                activities = await this._user1.GetActivities(0, 5);
                Assert.IsNotNull(activities);
                Assert.AreEqual(3, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }

        [Test]
        public async Task TestFlatFollowUnfollowPrivateKeepHistoryDefault()
        {
            var secret = this._client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(secret);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await secret.AddActivity(newActivity);



                await this._user1.FollowFeed(secret);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeed(secret);


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
            var secret = this._client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(secret);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await secret.AddActivity(newActivity);



                await this._user1.FollowFeed(secret);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeed(secret, false);


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
            var secret = this._client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(secret);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                var newActivity = new Stream.Activity("1", "test", "1");
                var response = await secret.AddActivity(newActivity);



                await this._user1.FollowFeed(secret);


                activities = await this._user1.GetActivities(0, 1);
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeed(secret, true);


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
        public async Task TestFlatFollowPrivateActivityCopyLimitDefault()
        {
            var secret = this._client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(secret, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Stream.Activity("1", "test", "1"), new Stream.Activity("1", "test", "2"), new Stream.Activity("1", "test", "3"), new Stream.Activity("1", "test", "4"), new Stream.Activity("1", "test", "5") };
                var response = await secret.AddActivities(newActivities);



                await this._user1.FollowFeed(secret);


                activities = await this._user1.GetActivities(0, 5);
                Assert.IsNotNull(activities);
                Assert.AreEqual(5, activities.Count());
            }
            else
            {
                Assert.Fail("Initial activity count not zero.");
            }
        }
        [Test]
        public async Task TestFlatFollowPrivateActivityCopyLimitNonDefault()
        {
            var secret = this._client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeed(secret, false);

            var activities = await this._user1.GetActivities(0, 1);

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Stream.Activity("1", "test", "1"), new Stream.Activity("1", "test", "2"), new Stream.Activity("1", "test", "3"), new Stream.Activity("1", "test", "4"), new Stream.Activity("1", "test", "5") };
                var response = await secret.AddActivities(newActivities);



                await this._user1.FollowFeed(secret, 3);


                activities = await this._user1.GetActivities(0, 5);
                Assert.IsNotNull(activities);
                Assert.AreEqual(3, activities.Count());
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



            var activities = await _not5.GetActivities(0, 2, marker: ActivityMarker.Mark().AllRead());
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var notActivity = activities.First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First() as NotificationActivity;
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);



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
            var feed = this._client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed1 = this._client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed2 = this._client.Feed("flat", System.Guid.NewGuid().ToString());

            // unfollow
            await feed1.UnfollowFeed(feed);
            await feed2.UnfollowFeed(feed);



            var response = await feed.Followers(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeed(feed);
            await feed2.FollowFeed(feed);



            response = await feed.Followers(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var first = response.First();
            Assert.AreEqual(first.FeedId, feed2.FeedId);
            Assert.AreEqual(first.TargetId, feed.FeedId);

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
            var feed = this._client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed1 = this._client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed2 = this._client.Feed("flat", System.Guid.NewGuid().ToString());

            // unfollow
            await feed1.UnfollowFeed(feed);
            await feed1.UnfollowFeed(feed2);



            var response = await feed1.Following(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeed(feed);
            await feed1.FollowFeed(feed2);



            response = await feed1.Following(0, 2);
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());
            Assert.AreEqual(response.First().FeedId, feed1.FeedId);
            Assert.AreEqual(response.First().TargetId, feed2.FeedId);
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

            await _agg4.FollowFeed(this._user1);

            var activities = await this._agg4.GetActivities(0);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(2, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await _agg4.UnfollowFeed(this._user1);
        }

        [Test]
        public async Task TestGetAggregate()
        {
            var newActivity1 = new Stream.Activity("1", "test", "1");
            var newActivity2 = new Stream.Activity("1", "test", "2");
            var response = await _user1.AddActivity(newActivity1);
            response = await _user1.AddActivity(newActivity2);

            await _agg4.FollowFeed(this._user1);

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

            await _agg4.UnfollowFeed(this._user1);
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



            await _agg4.FollowFeed(this._user1);


            var activities = await this._agg4.GetActivities(0);
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(1, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await _agg4.UnfollowFeed(this._user1);
        }


        [Test]
        public async Task TestBatchFollow()
        {
            await _client.Batch.FollowMany(new[]
            {
                new Follow(_user1, _flat3),
                new Follow(_user2, _flat3)
            });


            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._flat3.AddActivity(newActivity);

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
                new Follow(this._user1, this._flat3),
                new Follow(this._user2, this._flat3)
            }, 10);


            var newActivity = new Stream.Activity("1", "test", "1");
            var response = await this._flat3.AddActivity(newActivity);

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

        [Test]
        public async Task TestGetActivitiesByID()
        {
            var newActivity1 = new Stream.Activity("1", "test", "1");
            var newActivity2 = new Stream.Activity("1", "test", "2");
            var newActivity3 = new Stream.Activity("1", "other", "2");
            var addedActivities = new List<Stream.Activity>();

            var response = await this._user1.AddActivity(newActivity1);
            addedActivities.Add(response);
            response = await this._user2.AddActivity(newActivity2);
            addedActivities.Add(response);
            response = await this._flat3.AddActivity(newActivity3);
            addedActivities.Add(response);


            var activities = await this._client.Batch.GetActivities(addedActivities.Select(a => a.Id));
            Assert.IsNotNull(activities);
            Assert.AreEqual(addedActivities.Count, activities.Count());

            activities.ForEach(a =>
            {
                var found = addedActivities.Find(x => x.Id == a.Id);
                Assert.NotNull(found);
                Assert.AreEqual(found.Actor, a.Actor);
                Assert.AreEqual(found.Object, a.Object);
                Assert.AreEqual(found.Verb, a.Verb);
            });
        }

        [Test]
        public async Task TestGetActivitiesByForeignIDAndTime()
        {
            var newActivity1 = new Stream.Activity("1", "test", "1")
            {
                ForeignId = "fid-test-1",
                Time = DateTime.Parse("2000-08-16T16:32:32")
            };

            var newActivity2 = new Stream.Activity("1", "test", "2")
            {
                ForeignId = "fid-test-2",
                Time = DateTime.Parse("2000-08-17T16:32:32")
            };

            var newActivity3 = new Stream.Activity("1", "other", "2")
            {
                ForeignId = "fid-other-1",
                Time = DateTime.Parse("2000-08-19T16:32:32")
            };

            var addedActivities = new List<Stream.Activity>();

            var response = await this._user1.AddActivity(newActivity1);
            addedActivities.Add(response);
            response = await this._user2.AddActivity(newActivity2);
            addedActivities.Add(response);
            response = await this._flat3.AddActivity(newActivity3);
            addedActivities.Add(response);


            var activities = await this._client.Batch.GetActivities(null,
                addedActivities.Select(a => new Stream.ForeignIDTime(a.ForeignId, a.Time.Value)));
            Assert.IsNotNull(activities);
            Assert.AreEqual(addedActivities.Count, activities.Count());

            activities.ForEach(a =>
            {
                var found = addedActivities.Find(x => x.Id == a.Id);
                Assert.NotNull(found);
                Assert.AreEqual(found.Actor, a.Actor);
                Assert.AreEqual(found.Object, a.Object);
                Assert.AreEqual(found.Verb, a.Verb);
                Assert.AreEqual(found.ForeignId, a.ForeignId);
                Assert.AreEqual(found.Time, a.Time);
            });
        }

        [Test]
        public void TestCollectionsUpsert()
        {
            var data = new CollectionObject(System.Guid.NewGuid().ToString());
            data.SetData("hobbies", new List<string> { "eating", "coding" });

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Collections.Upsert("people", data);
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
                await this._client.Collections.UpsertMany("people", data);
            });
        }

        [Test]
        public async Task TestCollectionsSelect()
        {
            string id1 = System.Guid.NewGuid().ToString(),
            id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await this._client.Collections.UpsertMany("people", data);

            var result = await this._client.Collections.Select("people", id1);

            Assert.NotNull(result);
            Assert.AreEqual(data1.ID, result.ID);
            Assert.AreEqual(data1.GetData<List<string>>("hobbies"), result.GetData<List<string>>("hobbies"));
        }

        [Test]
        public async Task TestCollectionsSelectMany()
        {
            string id1 = System.Guid.NewGuid().ToString(),
            id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await this._client.Collections.UpsertMany("people", data);

            var results = await this._client.Collections.SelectMany("people", new string[] { id1, id2 });

            Assert.NotNull(results);
            Assert.AreEqual(data.Count, results.SafeCount());
            results.ForEach(r =>
            {
                var found = data.Find(x => x.ID == r.ID);
                Assert.NotNull(found);
                var key = r.ID == id1 ? "hobbies" : "vacation";
                Assert.AreEqual(found.GetData<List<string>>(key), r.GetData<List<string>>(key));
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

            await this._client.Collections.UpsertMany("people", data);

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Collections.Delete("people", id2);
            });

            var results = await this._client.Collections.SelectMany("people", new string[] { id1, id2 });

            Assert.NotNull(results);
            Assert.AreEqual(1, results.SafeCount());
            var result = results.First();
            Assert.AreEqual(id1, result.ID);
            Assert.AreEqual(data1.GetData<List<string>>("hobbies"), result.GetData<List<string>>("hobbies"));
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

            await this._client.Collections.UpsertMany("people", data);

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Collections.DeleteMany("people", new string[] { id1, id2 });
            });

            var results = await this._client.Collections.SelectMany("people", new string[] { id1, id2 });

            Assert.NotNull(results);
            Assert.AreEqual(0, results.SafeCount());
        }

        [Test]
        public async Task TestCollectionsCRUD()
        {
            var colData = new GenericData();
            colData.SetData("field", "value");
            colData.SetData("flag", true);

            //ADD
            CollectionObject collectionObject = await this._client.Collections.Add("col_test_crud", colData);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.ID));
            Assert.AreEqual("value", collectionObject.GetData<string>("field"));
            Assert.AreEqual(true, collectionObject.GetData<bool>("flag"));

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var o = await this._client.Collections.Add("col_test_crud", colData, collectionObject.ID);
            });

            //GET
            collectionObject = await this._client.Collections.Get("col_test_crud", collectionObject.ID);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.ID));
            Assert.AreEqual("value", collectionObject.GetData<string>("field"));
            Assert.AreEqual(true, collectionObject.GetData<bool>("flag"));

            //UPDATE
            var newData = new GenericData();
            newData.SetData("new", "stuff");
            newData.SetData("arr", new string[] { "a", "b" });
            collectionObject = await this._client.Collections.Update("col_test_crud", collectionObject.ID, newData);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.ID));
            Assert.AreEqual("stuff", collectionObject.GetData<string>("new"));
            Assert.AreEqual(new string[] { "a", "b" }, collectionObject.GetData<string[]>("arr"));

            //DELETE
            await this._client.Collections.Delete("col_test_crud", collectionObject.ID);

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var o = await this._client.Collections.Get("col_test_crud", collectionObject.ID);
            });
        }

        [Test]
        public async Task TestActivityPartialUpdateByID()
        {
            var act = new Stream.Activity("upd", "test", "1")
            {
                ForeignId = System.Guid.NewGuid().ToString(),
                Time = DateTime.UtcNow
            };
            act.SetData("custom_thing", "12345");
            act.SetData("custom_thing2", "foobar");
            act.SetData("custom_thing3", "some thing");

            var insertedAct = await this._user1.AddActivity(act);
            Assert.IsNotNull(insertedAct);
            Assert.AreEqual("12345", insertedAct.GetData<string>("custom_thing"));
            Assert.AreEqual("foobar", insertedAct.GetData<string>("custom_thing2"));
            Assert.AreEqual("some thing", insertedAct.GetData<string>("custom_thing3"));

            var set = new Stream.GenericData();
            set.SetData("custom_thing", "abcdef");

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.ActivityPartialUpdate(insertedAct.Id, null, set);
            });

            var updatedAct = (await this._client.Batch.GetActivities(new string[] { insertedAct.Id })).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual("abcdef", updatedAct.GetData<string>("custom_thing"));

            var unset = new string[] { "custom_thing2" };

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.ActivityPartialUpdate(insertedAct.Id, null, null, unset);
            });

            updatedAct = (await this._client.Batch.GetActivities(new string[] { insertedAct.Id })).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing2"));

            set.SetData("custom_thing", "zyx");
            unset[0] = "custom_thing3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.ActivityPartialUpdate(insertedAct.Id, null, set, unset);
            });

            updatedAct = (await this._client.Batch.GetActivities(new string[] { insertedAct.Id })).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing3"));
            Assert.AreEqual("zyx", updatedAct.GetData<string>("custom_thing"));
        }

        [Test]
        public async Task TestActivityPartialUpdateByForeignIDTime()
        {
            var fidTime = new Stream.ForeignIDTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);
            var act = new Stream.Activity("upd", "test", "1")
            {
                ForeignId = fidTime.ForeignID,
                Time = fidTime.Time
            };
            act.SetData("custom_thing", "12345");
            act.SetData("custom_thing2", "foobar");
            act.SetData("custom_thing3", "some thing");

            var insertedAct = await this._user1.AddActivity(act);
            Assert.IsNotNull(insertedAct);
            Assert.AreEqual("12345", insertedAct.GetData<string>("custom_thing"));
            Assert.AreEqual("foobar", insertedAct.GetData<string>("custom_thing2"));
            Assert.AreEqual("some thing", insertedAct.GetData<string>("custom_thing3"));

            var set = new Stream.GenericData();
            set.SetData("custom_thing", "abcdef");

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.ActivityPartialUpdate(null, fidTime, set);
            });

            var updatedAct = (await this._client.Batch.GetActivities(new string[] { insertedAct.Id })).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual("abcdef", updatedAct.GetData<string>("custom_thing"));

            var unset = new string[] { "custom_thing2" };

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.ActivityPartialUpdate(null, fidTime, null, unset);
            });

            updatedAct = (await this._client.Batch.GetActivities(new string[] { insertedAct.Id })).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing2"));

            set.SetData("custom_thing", "zyx");
            unset[0] = "custom_thing3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.ActivityPartialUpdate(null, fidTime, set, unset);
            });

            updatedAct = (await this._client.Batch.GetActivities(new string[] { insertedAct.Id })).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing3"));
            Assert.AreEqual("zyx", updatedAct.GetData<string>("custom_thing"));
        }

        [Test]
        public async Task TestUpdateToTargets()
        {
            var fidTime = new Stream.ForeignIDTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);

            var targets = new List<string>()
            {
                "flat:" + Guid.NewGuid().ToString(),
                "user:" + Guid.NewGuid().ToString(),
            };

            var act = new Stream.Activity("upd", "test", "1")
            {
                ForeignId = fidTime.ForeignID,
                Time = fidTime.Time,
                To = targets
            };

            var insertedAct = await this._user1.AddActivity(act);
            Assert.AreEqual(2, insertedAct.To.Count);

            //add 1
            var add = "user:" + Guid.NewGuid().ToString();
            var updateResp = await this._user1.UpdateActivityToTargets(fidTime, new string[] { add });
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(1, updateResp.Added.Count);
            Assert.AreEqual(add, updateResp.Added[0]);
            Assert.AreEqual(3, updateResp.Activity.To.Count);
            Assert.IsNotNull(updateResp.Activity.To.ToList().Find(t => t == add));

            var updatedAct = (await this._user1.GetActivities(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(3, updatedAct.To.Count);
            Assert.IsNotNull(updatedAct.To.ToList().Find(t => t == add));

            //remove 1
            var remove = targets[0];
            updateResp = await this._user1.UpdateActivityToTargets(fidTime, null, null, new string[] { remove });
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(1, updateResp.Removed.Count);
            Assert.AreEqual(remove, updateResp.Removed[0]);
            Assert.AreEqual(2, updateResp.Activity.To.Count);
            Assert.IsNull(updateResp.Activity.To.ToList().Find(t => t == remove));

            updatedAct = (await this._user1.GetActivities(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(2, updatedAct.To.Count);
            Assert.IsNull(updatedAct.To.ToList().Find(t => t == remove));

            //new ones
            var newOnes = new List<string>()
            {
                "flat:" + Guid.NewGuid().ToString(),
                "user:" + Guid.NewGuid().ToString(),
            };
            updateResp = await this._user1.UpdateActivityToTargets(fidTime, null, newOnes);
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(2, updateResp.Activity.To.Count);
            Assert.AreEqual(2, updateResp.Added.Count);
            Assert.AreEqual(2, updateResp.Added.ToList().FindAll(t => newOnes.Contains(t)).Count);
            updatedAct = (await this._user1.GetActivities(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(2, updatedAct.To.Count);
            Assert.AreEqual(2, updatedAct.To.ToList().FindAll(t => newOnes.Contains(t)).Count);
        }

        [Test]
        public async Task TestBatchPartialUpdate()
        {
            var fidTime1 = new Stream.ForeignIDTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);
            var act1 = new Stream.Activity("upd", "test", "1")
            {
                ForeignId = fidTime1.ForeignID,
                Time = fidTime1.Time
            };
            act1.SetData("custom_thing", "12345");
            act1.SetData("custom_thing2", "foobar");
            act1.SetData("custom_thing3", "some thing");
            var fidTime2 = new Stream.ForeignIDTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-3));
            var act2 = new Stream.Activity("upd", "test", "1")
            {
                ForeignId = fidTime2.ForeignID,
                Time = fidTime2.Time
            };
            act2.SetData("custom_flag", "val1");
            act2.SetData("custom_flag2", "val2");
            act2.SetData("custom_flag3", "val3");

            var fidTime3 = new Stream.ForeignIDTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-6));
            var act3 = new Stream.Activity("upd", "test", "1")
            {
                ForeignId = fidTime3.ForeignID,
                Time = fidTime3.Time
            };
            var customData = new Dictionary<string, string>()
            {
                {"name", "BOB"},
                {"address", "90210"},
                {"email", "bob@bobobo.com"},
            };
            act3.SetData("details", customData);

            var response = await this._user1.AddActivities(new Stream.Activity[] { act1, act2, act3 });
            var insertedActs = response.ToArray();

            //FID TIME
            var upd1 = new Stream.ActivityPartialUpdateRequestObject()
            {
                ForeignIDTime = fidTime1,
                Unset = new string[] { "custom_thing3" }
            };

            var set = new GenericData();
            set.SetData("details.address", "nowhere");
            var upd2 = new Stream.ActivityPartialUpdateRequestObject()
            {
                ForeignIDTime = fidTime3,
                Set = set
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.ActivitiesPartialUpdate(new Stream.ActivityPartialUpdateRequestObject[] { upd1, upd2 });
            });

            var updatedActs = (await this._user1.GetActivities()).ToArray();

            Assert.IsNull(updatedActs[0].GetData<string>("custom_thing3"));
            var extraData = updatedActs[2].GetData<Dictionary<string, string>>("details");
            Assert.AreEqual("nowhere", extraData["address"]);

            //ID
            set = new GenericData();
            set.SetData("custom_flag2", "foobar");
            upd1 = new Stream.ActivityPartialUpdateRequestObject()
            {
                ID = insertedActs[1].Id,
                Set = set
            };
            upd2 = new Stream.ActivityPartialUpdateRequestObject()
            {
                ID = insertedActs[2].Id,
                Unset = new string[] { "details.name" }
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.ActivitiesPartialUpdate(new Stream.ActivityPartialUpdateRequestObject[] { upd1, upd2 });
            });

            updatedActs = (await this._user1.GetActivities()).ToArray();

            Assert.AreEqual("foobar", updatedActs[1].GetData<string>("custom_flag2"));
            extraData = updatedActs[2].GetData<Dictionary<string, string>>("details");
            Assert.False(extraData.ContainsKey("name"));
        }

        [Test]
        public async Task TestBatchUpdateActivity()
        {
            var activity = new Stream.Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };
            activity.SetData("custom", "field");
            var insertedActivity = await this._user1.AddActivity(activity);

            activity.Target = "timmy";
            activity.SetData("custom", "data");
            activity.SetData("another", "thing");

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.UpdateActivity(activity);
            });

            var updatedActivity = (await this._user1.GetActivities(0, 1)).FirstOrDefault();
            Assert.NotNull(updatedActivity);
            Assert.AreEqual(insertedActivity.Id, updatedActivity.Id);
            Assert.AreEqual(activity.Target, updatedActivity.Target);
            Assert.AreEqual(activity.GetData<string>("custom"), updatedActivity.GetData<string>("custom"));
            Assert.AreEqual(activity.GetData<string>("another"), updatedActivity.GetData<string>("another"));
        }

        [Test]
        public async Task TestBatchUpdateActivities()
        {
            var activity = new Stream.Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };
            activity.SetData("custom", "field");
            var activity2 = new Stream.Activity("user:123", "posts", "selfie")
            {
                ForeignId = "selfie:2",
                Time = DateTime.UtcNow,
            };

            var insertedActivity = await this._user1.AddActivity(activity);
            var insertedActivity2 = await this._flat3.AddActivity(activity2);

            activity.SetData("custom", "data");
            activity.Target = null;
            activity2.SetData("new-stuff", new int[] { 3, 2, 1 });
            activity2.Actor = "user:3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Batch.UpdateActivities(new Stream.Activity[] { activity, activity2 });
            });

            var updatedActivity = (await this._user1.GetActivities(0, 1)).FirstOrDefault();
            Assert.NotNull(updatedActivity);
            Assert.AreEqual(insertedActivity.Id, updatedActivity.Id);
            Assert.AreEqual(string.Empty, updatedActivity.Target);
            Assert.AreEqual(activity.GetData<string>("custom"), updatedActivity.GetData<string>("custom"));

            var updatedActivity2 = (await this._flat3.GetActivities(0, 1)).FirstOrDefault();
            Assert.NotNull(updatedActivity2);
            Assert.AreEqual(insertedActivity2.Id, updatedActivity2.Id);
            Assert.AreEqual(activity2.Actor, updatedActivity2.Actor);
            Assert.AreEqual(activity2.GetData<int[]>("custom"), updatedActivity2.GetData<int[]>("custom"));
        }

        [Test]
        public async Task TestReactions()
        {
            var a = new Stream.Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };

            var activity = await this._user1.AddActivity(a);

            var data = new Dictionary<string, object>()
            {
                {"field", "value"},
                {"number", 2}
            };

            Reaction r = null;
            // Add reaction
            Assert.DoesNotThrowAsync(async () =>
            {
                r = await this._client.Reactions.Add("like", activity.Id, "bobby", data);
            });

            Assert.NotNull(r);
            Assert.AreEqual(r.ActivityID, activity.Id);
            Assert.AreEqual(r.Kind, "like");
            Assert.AreEqual(r.UserID, "bobby");
            Assert.AreEqual(r.Data, data);
            Assert.True(r.CreatedAt.HasValue);
            Assert.True(r.UpdatedAt.HasValue);
            Assert.IsNotEmpty(r.ID);

            // get reaction
            Reaction r2 = null;
            Assert.DoesNotThrowAsync(async () =>
            {
                r2 = await this._client.Reactions.Get(r.ID);
            });

            Assert.NotNull(r2);
            Assert.AreEqual(r2.ActivityID, r.ActivityID);
            Assert.AreEqual(r2.Kind, "like");
            Assert.AreEqual(r2.UserID, "bobby");
            Assert.AreEqual(r2.Data, r.Data);
            Assert.AreEqual(r2.ID, r.ID);

            // Update reaction
            data["number"] = 321;
            data["new"] = "field";
            data.Remove("field");

            var beforeTime = r.UpdatedAt.Value;
            Assert.DoesNotThrowAsync(async () =>
            {
                r2 = await this._client.Reactions.Update(r.ID, data);
            });
            Assert.NotNull(r2);
            Assert.False(r2.Data.ContainsKey("field"));
            object n;
            Assert.True(r2.Data.TryGetValue("number", out n));
            Assert.AreEqual((Int64)n, 321);
            Assert.True(r2.Data.ContainsKey("new"));

            // Add children
            var c1 = await this._client.Reactions.AddChild(r, "upvote", "tommy");
            var c2 = await this._client.Reactions.AddChild(r, "downvote", "timmy");
            var c3 = await this._client.Reactions.AddChild(r, "upvote", "jimmy");

            var parent = await this._client.Reactions.Get(r.ID);

            Assert.AreEqual(parent.ChildrenCounters["upvote"], 2);
            Assert.AreEqual(parent.ChildrenCounters["downvote"], 1);

            Assert.IsTrue(parent.LatestChildren["upvote"].Select(x => x.ID).Contains(c1.ID));
            Assert.IsTrue(parent.LatestChildren["upvote"].Select(x => x.ID).Contains(c3.ID));
            Assert.IsTrue(parent.LatestChildren["downvote"].Select(x => x.ID).Contains(c2.ID));

            // Delete reaction

            Assert.DoesNotThrowAsync(async () =>
            {
                await this._client.Reactions.Delete(r.ID);
            });

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var r3 = await this._client.Reactions.Get(r.ID);
            });
        }

        [Test]
        public async Task TestReactionPagination()
        {
            var a = new Stream.Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };

            var activity = await this._user1.AddActivity(a);

            a.Time = DateTime.UtcNow;
            a.ForeignId = "cake:123";
            var activity2 = await this._user1.AddActivity(a);

            var data = new Dictionary<string, object>()
            {
                {"field", "value"},
                {"number", 2}
            };

            var userId = Guid.NewGuid().ToString();

            var r1 = await this._client.Reactions.Add("like", activity.Id, userId, data);
            var r2 = await this._client.Reactions.Add("comment", activity.Id, userId, data);
            var r3 = await this._client.Reactions.Add("like", activity.Id, "bob", data);

            var r4 = await this._client.Reactions.AddChild(r3, "upvote", "tom", data);
            var r5 = await this._client.Reactions.AddChild(r3, "upvote", "mary", data);

            // activity id
            var filter = ReactionFiltering.Default;
            var pagination = ReactionPagination.By.ActivityID(activity.Id).Kind("like");

            var reactionsByActivity = await this._client.Reactions.Filter(filter, pagination);
            Assert.AreEqual(2, reactionsByActivity.Count());

            var r = (List<Reaction>)reactionsByActivity;
            var actual = r.Find(x => x.ID == r1.ID);

            Assert.NotNull(actual);
            Assert.AreEqual(r1.ID, actual.ID);
            Assert.AreEqual(r1.Kind, actual.Kind);
            Assert.AreEqual(r1.ActivityID, actual.ActivityID);

            actual = r.Find(x => x.ID == r3.ID);

            Assert.NotNull(actual);
            Assert.AreEqual(r3.ID, actual.ID);
            Assert.AreEqual(r3.Kind, actual.Kind);
            Assert.AreEqual(r3.ActivityID, actual.ActivityID);

            //with limit
            reactionsByActivity = await this._client.Reactions.Filter(filter.WithLimit(1), pagination);
            Assert.AreEqual(1, reactionsByActivity.Count());

            //with data
            var reactionsByActivityWithData = await this._client.Reactions.FilterWithActivityData(filter.WithLimit(1), pagination);
            Assert.AreEqual(1, reactionsByActivity.Count());
            Assert.AreEqual(data, reactionsByActivity.FirstOrDefault().Data);

            // user id
            filter = ReactionFiltering.Default;
            pagination = ReactionPagination.By.UserID(userId);

            var reactionsByUser = await this._client.Reactions.Filter(filter, pagination);
            Assert.AreEqual(2, reactionsByUser.Count());

            r = (List<Reaction>)reactionsByUser;
            actual = r.Find(x => x.ID == r1.ID);

            Assert.NotNull(actual);
            Assert.AreEqual(r1.ID, actual.ID);
            Assert.AreEqual(r1.Kind, actual.Kind);
            Assert.AreEqual(r1.ActivityID, actual.ActivityID);

            actual = r.Find(x => x.ID == r2.ID);

            Assert.NotNull(actual);
            Assert.AreEqual(r2.ID, actual.ID);
            Assert.AreEqual(r2.Kind, actual.Kind);
            Assert.AreEqual(r2.ActivityID, actual.ActivityID);

            // reaction id
            filter = ReactionFiltering.Default;
            pagination = ReactionPagination.By.Kind("upvote").ReactionID(r3.ID);

            var reactionsByParent = await this._client.Reactions.Filter(filter, pagination);
            Assert.AreEqual(2, reactionsByParent.Count());

            r = (List<Reaction>)reactionsByParent;
            actual = r.Find(x => x.ID == r4.ID);

            Assert.NotNull(actual);
            Assert.AreEqual(r4.ID, actual.ID);
            Assert.AreEqual(r4.Kind, actual.Kind);
            Assert.AreEqual(r4.ActivityID, actual.ActivityID);
            Assert.AreEqual(r4.UserID, actual.UserID);

            actual = r.Find(x => x.ID == r5.ID);

            Assert.NotNull(actual);
            Assert.AreEqual(r5.ID, actual.ID);
            Assert.AreEqual(r5.Kind, actual.Kind);
            Assert.AreEqual(r5.ActivityID, actual.ActivityID);
            Assert.AreEqual(r5.UserID, actual.UserID);
        }

        [Test]
        public async Task TestUsers()
        {
            //Create user
            var userID = Guid.NewGuid().ToString();
            var userData = new Dictionary<string, object>()
            {
                {"field", "value"},
                {"is_admin", true},
            };

            User u = null;
            Assert.DoesNotThrowAsync(async () =>
            {
                u = await this._client.Users.Add(userID, userData);
            });

            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userID, u.ID);
            Assert.AreEqual(userData, u.Data);

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                u = await this._client.Users.Add(userID, userData);
            });

            var newUserData = new Dictionary<string, object>()
            {
                {"field", "othervalue"},
            };
            Assert.DoesNotThrowAsync(async () =>
            {
                u = await this._client.Users.Add(userID, newUserData, true);
            });
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userID, u.ID);
            Assert.AreEqual(userData, u.Data);

            //Get user
            u = await this._client.Users.Get(userID);
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userID, u.ID);
            Assert.AreEqual(userData, u.Data);

            // Update user
            u = await this._client.Users.Update(userID, newUserData);
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userID, u.ID);
            Assert.AreEqual(newUserData, u.Data);

            //Delete user
            await this._client.Users.Delete(userID);

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var x = await this._client.Users.Get(userID);
            });
        }

        [Test]
        public async Task TestEnrich_Collection()
        {
            var c = new CollectionObject(Guid.NewGuid().ToString());
            c.SetData("field", "testing_value");
            await this._client.Collections.Upsert("items", c);
            var cRef = Stream.Collections.Ref("items", c);

            var a = new Stream.Activity("actor-1", "add", cRef);
            await this._user1.AddActivity(a);

            var plain = await this._user1.GetFlatActivities();
            Assert.AreEqual(cRef, plain.Results.First().Object);

            var enriched = await this._user1.GetEnrichedFlatActivities();
            Assert.AreEqual(1, enriched.Results.Count());

            var act = enriched.Results.First();
            Assert.IsFalse(act.Actor.IsEnriched);
            Assert.AreEqual("actor-1", act.Actor.Raw);
            Assert.IsTrue(act.Object.IsEnriched);
            Assert.AreEqual(c.ID, act.Object.Enriched.GetData<string>("id"));
            Assert.AreEqual("testing_value", act.Object.Enriched.GetData<Dictionary<string, string>>("data")["field"]);
        }

        [Test]
        public async Task TestEnrich_User()
        {
            var userData = new Dictionary<string, object>()
            {
                {"is_admin", true},
                {"nickname","bobby"}
            };
            var u = await this._client.Users.Add(Guid.NewGuid().ToString(), userData);
            var uRef = u.Ref();

            var a = new Stream.Activity(uRef, "add", "post");
            await this._user1.AddActivity(a);

            var plain = await this._user1.GetFlatActivities();
            Assert.AreEqual(uRef, plain.Results.First().Actor);

            var enriched = await this._user1.GetEnrichedFlatActivities();

            Assert.AreEqual(1, enriched.Results.Count());

            var act = enriched.Results.First();
            Assert.IsFalse(act.Object.IsEnriched);
            Assert.AreEqual("post", act.Object.Raw);
            Assert.IsTrue(act.Actor.IsEnriched);
            Assert.AreEqual(u.ID, act.Actor.Enriched.GetData<string>("id"));
            Assert.AreEqual(userData, act.Actor.Enriched.GetData<Dictionary<string, object>>("data"));
        }

        [Test]
        public async Task TestEnrich_OwnReaction()
        {
            var a = new Stream.Activity("johhny", "add", "post");
            var act = await this._user1.AddActivity(a);
            var reaction = await this._client.Reactions.Add("like", act.Id, "johhny");

            var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Own()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.OwnReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.ID, enrichedAct.OwnReactions[reaction.Kind].FirstOrDefault().ID);
            Assert.AreEqual(reaction.Kind, enrichedAct.OwnReactions[reaction.Kind].FirstOrDefault().Kind);
            Assert.AreEqual(reaction.UserID, enrichedAct.OwnReactions[reaction.Kind].FirstOrDefault().UserID);
        }

        [Test]
        public async Task TestEnrich_LatestReactions()
        {
            var a = new Stream.Activity("johhny", "add", "post");
            var act = await this._user1.AddActivity(a);
            var reaction = await this._client.Reactions.Add("like", act.Id, "johhny");

            var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Recent()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.ID, enrichedAct.LatestReactions[reaction.Kind].FirstOrDefault().ID);
            Assert.AreEqual(reaction.Kind, enrichedAct.LatestReactions[reaction.Kind].FirstOrDefault().Kind);
            Assert.AreEqual(reaction.UserID, enrichedAct.LatestReactions[reaction.Kind].FirstOrDefault().UserID);

            var comment = await this._client.Reactions.Add("comment", act.Id, "bobby");
            await this._client.Reactions.Add("comment", act.Id, "tony");
            await this._client.Reactions.Add("comment", act.Id, "rupert");

            enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Recent().KindFilter("comment").KindFilter("upvote")));

            Assert.AreEqual(1, enriched.Results.Count());

            enrichedAct = enriched.Results.First();

            Assert.False(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.True(enrichedAct.LatestReactions.ContainsKey(comment.Kind));

            enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Recent().RecentLimit(1)));

            Assert.AreEqual(1, enriched.Results.Count());

            enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.True(enrichedAct.LatestReactions.ContainsKey(comment.Kind));

            Assert.AreEqual(1, enrichedAct.LatestReactions[reaction.Kind].Count());
            Assert.AreEqual(1, enrichedAct.LatestReactions[comment.Kind].Count());
        }

        [Test]
        public async Task TestEnrich_ReactionCounts()
        {
            var a = new Stream.Activity("johhny", "add", "post");
            var act = await this._user1.AddActivity(a);
            var reactionLike = await this._client.Reactions.Add("like", act.Id, "johhny");
            var reactionComment = await this._client.Reactions.Add("comment", act.Id, "johhny");
            var reactionLike2 = await this._client.Reactions.Add("like", act.Id, "timmeh");

            var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Counts()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.ReactionCounts.ContainsKey(reactionLike.Kind));
            Assert.True(enrichedAct.ReactionCounts.ContainsKey(reactionComment.Kind));
            Assert.AreEqual(2, enrichedAct.ReactionCounts[reactionLike.Kind]);
            Assert.AreEqual(1, enrichedAct.ReactionCounts[reactionComment.Kind]);
        }

        [Test]
        public async Task TestEnrich()
        {
            var a = new Stream.Activity("johhny", "add", "post");
            var act = await this._user1.AddActivity(a);
            var reaction = await this._client.Reactions.Add("like", act.Id, "johhny");

            var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Recent().Own().Counts()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.ID, enrichedAct.LatestReactions[reaction.Kind].FirstOrDefault().ID);
            Assert.AreEqual(reaction.Kind, enrichedAct.LatestReactions[reaction.Kind].FirstOrDefault().Kind);
            Assert.AreEqual(reaction.UserID, enrichedAct.LatestReactions[reaction.Kind].FirstOrDefault().UserID);

            Assert.True(enrichedAct.OwnReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.ID, enrichedAct.OwnReactions[reaction.Kind].FirstOrDefault().ID);
            Assert.AreEqual(reaction.Kind, enrichedAct.OwnReactions[reaction.Kind].FirstOrDefault().Kind);
            Assert.AreEqual(reaction.UserID, enrichedAct.OwnReactions[reaction.Kind].FirstOrDefault().UserID);

            Assert.True(enrichedAct.ReactionCounts.ContainsKey(reaction.Kind));
            Assert.AreEqual(1, enrichedAct.ReactionCounts[reaction.Kind]);
        }

        [Test]
        [Ignore("Not always needed, set credentials to run when needed")]
        public async Task ReadPersonalization()
        {
            var _p = new Stream.StreamClient(
                "some_key",
                "some_secret",
                new Stream.StreamClientOptions()
                {
                    Location = Stream.StreamApiLocation.Dublin,
                    PersonalizationLocation = Stream.StreamApiLocation.USEast,
                    Timeout = 10000,
                    PersonalizationTimeout = 10000
                });

            var response = await _p.Personalization.Get("etoro_newsfeed", new Dictionary<string, object>()
                {
                    {"feed_slug", "newsfeed"},
                    {"user_id", "crembo"},
                    {"limit", 20},
                    {"ranking", "etoro"}
                });

            var d = new Dictionary<string, object>(response);
            Assert.AreEqual(41021, d["app_id"]);
            Assert.True(d.ContainsKey("duration"));
            Assert.True(d.ContainsKey("results"));
        }
    }
}
