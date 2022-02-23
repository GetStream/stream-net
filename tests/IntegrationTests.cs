using NUnit.Framework;
using Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Stream.Models;

namespace StreamNetTests
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
            _client = Credentials.Instance.Client;
            _user1 = _client.Feed("user", System.Guid.NewGuid().ToString());
            _user2 = _client.Feed("user", System.Guid.NewGuid().ToString());
            _flat3 = _client.Feed("flat", System.Guid.NewGuid().ToString());
            _agg4 = _client.Feed("aggregate", System.Guid.NewGuid().ToString());
            _not5 = _client.Feed("notification", System.Guid.NewGuid().ToString());
        }

        [Test]
        public async Task TestAddActivity()
        {
            var newActivity = new Activity("1", "test", "1");
            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var newActivity = new Activity("1", "test", "1")
            {
                Time = now
            };
            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData("complex", new[] { "tommaso", "thierry", "shawn" });
            newActivity.SetData("special_json", new { StuffOneTwo = "thing" }, new JsonSerializer
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new KebabCaseNamingStrategy() }
            });
            newActivity.SetData(new Dictionary<string, object>
            {
                { "dictkey", "dictvalue" }
            });
            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<String[]>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual(3, complex.Length);
            Assert.AreEqual("shawn", complex[2]);

            Assert.AreEqual("thing", first.GetData<JObject>("special_json")["stuff-one-two"].Value<string>());

            Assert.AreEqual("dictvalue", first.GetData<string>("dictkey"));
        }

        [Test]
        public async Task TestAddActivityWithString()
        {
            int second = 2;
            double third = 3;
            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData("complex", "string");
            newActivity.SetData("second", second);
            newActivity.SetData("third", third);
            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var dict = new Dictionary<String, String>()
            {
                {"test1","shawn"},
                {"test2", "wedge"}
            };

            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData("complex", dict);

            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<Dictionary<String, String>>("complex");
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
            var dict = new Dictionary<string, object>()
            {
                {"test1", "shawn"},
                {"test2", "wedge"},
                {"test3", 42}
            };

            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData("complex", dict);
            newActivity.SetData("second", second);
            newActivity.SetData("third", third);
            newActivity.SetData("customc", new CustomClass()
            {
                TestString = "string",
                TestInt = 123,
                TestDouble = 42.2
            });

            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<Dictionary<string, object>>("complex");
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
            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData<string>("stringint", "42");
            newActivity.SetData<string>("stringdouble", "42.2");
            newActivity.SetData<string>("stringcomplex", "{ \"test1\": 1, \"test2\": \"testing\" }");

            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            Assert.AreEqual(42, first.GetData<int>("stringint"));

            Assert.AreEqual(42.2, first.GetData<double>("stringdouble"));

            var complex = first.GetData<Dictionary<string, object>>("stringcomplex");
            Assert.IsNotNull(complex);
        }

        [Test]
        public async Task TestAddActivityTo()
        {
            var newActivity = new Activity("multi1", "test", "1")
            {
                To = new List<String>("flat:remotefeed1".Yield())
            };
            var addedActivity = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(addedActivity);
            Assert.IsNotNull(addedActivity.To);
            Assert.AreEqual(1, addedActivity.To.CountOrFallback());
            Assert.AreEqual("flat:remotefeed1", addedActivity.To.First());


            var activities = (await _client.Feed("flat", "remotefeed1").GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual("multi1", first.Actor);
        }

        [Test]
        public async Task TestAddActivities()
        {
            var newActivities = new[]
            {
                new Activity("multi1","test","1"),
                new Activity("multi2","test","2")
            };

            var response = (await this._user1.AddActivitiesAsync(newActivities)).Activities;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());



            var activities = (await this._user1.GetActivitiesAsync(0, 2)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            Assert.AreEqual(response.Skip(1).First().Id, activities.First().Id);
            Assert.AreEqual(response.First().Id, activities.Skip(1).First().Id);
        }

        [Test]
        public async Task TestUpdateActivity()
        {
            var newActivity = new Activity("1", "test", "1")
            {
                ForeignId = "post:1",
                Time = DateTime.UtcNow
            };
            newActivity.SetData<string>("myData", "1");
            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            first.Actor = "editedActor1";
            first.Object = "editedOject1";
            first.Verb = "editedVerbTest";
            first.SetData<string>("myData", "editedMyData1");

            await this._user1.UpdateActivityAsync(first);



            activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var newActivities = new[]
            {
                new Activity("multi1", "test", "1")
                {
                    ForeignId = "post:1",
                    Time = DateTime.UtcNow
                },
                new Activity("multi2", "test", "2")
                {
                    ForeignId = "post:2",
                    Time = DateTime.UtcNow
                }
            };

            var response = (await this._user1.AddActivitiesAsync(newActivities)).Activities;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());



            var activities = (await this._user1.GetActivitiesAsync(0, 2)).Results.ToArray();
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

            await this._user1.UpdateActivitiesAsync(activities);



            var editedActivities = (await this._user1.GetActivitiesAsync(0, 2)).Results.ToArray();
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
            var newActivity = new Activity("1", "test", "1");
            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.FirstOrDefault();
            Assert.AreEqual(response.Id, first.Id);

            await this._user1.RemoveActivityAsync(first.Id);



            var nextActivities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(nextActivities);
            Assert.IsFalse(nextActivities.Any(na => na.Id == first.Id));
        }

        [Test]
        public async Task TestRemoveActivityByForeignId()
        {
            var fid = "post:42";
            var newActivity = new Activity("1", "test", "1")
            {
                ForeignId = fid
            };

            var response = await this._user1.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual(fid, activities.First().ForeignId);

            await this._user1.RemoveActivityAsync(fid, true);



            activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.AreEqual(0, activities.Count());
        }

        [Test]
        public async Task TestGet()
        {
            var newActivity = new Activity("1", "test", "1");
            var first = await this._user1.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "2");
            var second = await this._user1.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "3");
            var third = await this._user1.AddActivityAsync(newActivity);



            var activities = (await this._user1.GetActivitiesAsync(0, 2)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());
            Assert.AreEqual(third.Id, activities.First().Id);
            Assert.AreEqual(second.Id, activities.Skip(1).First().Id);

            activities = (await this._user1.GetActivitiesAsync(1, 2)).Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            //$id_offset =  ['id_lt' => $third_id];
            activities = (await this._user1.GetActivitiesAsync(0, 2, FeedFilter.Where().IdLessThan(third.Id))).Results;
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestGetFlatActivities()
        {
            var newActivity = new Activity("1", "test", "1");
            var first = await this._user1.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "2");
            var second = await this._user1.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "test", "3");
            var third = await this._user1.AddActivityAsync(newActivity);



            var response = await this._user1.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2));
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Duration);
            var activities = response.Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());
            Assert.AreEqual(third.Id, activities.First().Id);
            Assert.AreEqual(second.Id, activities.Skip(1).First().Id);

            response = await this._user1.GetFlatActivitiesAsync(GetOptions.Default.WithOffset(1).WithLimit(2));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            response = await this._user1.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2).WithFilter(FeedFilter.Where().IdLessThan(third.Id)));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);

            response = await this._user1.GetFlatActivitiesAsync(GetOptions.Default.WithLimit(2).WithSession("dummy").WithFilter(FeedFilter.Where().IdLessThan(third.Id)));
            activities = response.Results;
            Assert.AreEqual(second.Id, activities.First().Id);
        }

        [Test]
        public async Task TestFlatFollowUnfollowKeepHistoryDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(this._flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this._flat3.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(this._flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeedAsync(this._flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            await this._user1.UnfollowFeedAsync(this._flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this._flat3.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(this._flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeedAsync(this._flat3, false);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            await this._user1.UnfollowFeedAsync(this._flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            var newActivity = new Activity("1", "test", "1");
            var response = await this._flat3.AddActivityAsync(newActivity);

            await this._user1.FollowFeedAsync(this._flat3);

            activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);

            //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
            await this._user1.UnfollowFeedAsync(this._flat3, true);


            activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
        }
        [Test]
        public async Task TestFlatFollowActivityCopyLimitDefault()
        {
            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(this._flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await this._flat3.AddActivitiesAsync(newActivities);



                await this._user1.FollowFeedAsync(this._flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 5)).Results;
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
            await this._user1.UnfollowFeedAsync(this._flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await this._flat3.AddActivitiesAsync(newActivities);



                await this._user1.FollowFeedAsync(this._flat3, 3);


                activities = (await this._user1.GetActivitiesAsync(0, 5)).Results;
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
            await this._user1.UnfollowFeedAsync(_flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this._flat3.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(_flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeedAsync(_flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            await this._user1.UnfollowFeedAsync(_flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this._flat3.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(_flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeedAsync(_flat3, false);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            await this._user1.UnfollowFeedAsync(_flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await this._flat3.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(_flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeedAsync(_flat3, true);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            await this._user1.UnfollowFeedAsync(_flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await this._flat3.AddActivitiesAsync(newActivities);



                await this._user1.FollowFeedAsync(_flat3);


                activities = (await this._user1.GetActivitiesAsync(0, 5)).Results;
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
            await this._user1.UnfollowFeedAsync(_flat3, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await this._flat3.AddActivitiesAsync(newActivities);



                await this._user1.FollowFeedAsync(_flat3, 3);


                activities = (await this._user1.GetActivitiesAsync(0, 5)).Results;
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
            var secret = _client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(secret);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await secret.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(secret);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, do not pass the keepHistory param, expect that it defaults to false and existing activities will be removed
                await this._user1.UnfollowFeedAsync(secret);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var secret = _client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(secret);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await secret.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(secret);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = false, expect that existing activities will be removed
                await this._user1.UnfollowFeedAsync(secret, false);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var secret = _client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(secret);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                var newActivity = new Activity("1", "test", "1");
                var response = await secret.AddActivityAsync(newActivity);



                await this._user1.FollowFeedAsync(secret);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
                Assert.IsNotNull(activities);
                Assert.AreEqual(1, activities.Count());
                Assert.AreEqual(response.Id, activities.First().Id);

                //Unfollow, pass the keepHistory param = true, expect that existing activities will be retained
                await this._user1.UnfollowFeedAsync(secret, true);


                activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
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
            var secret = _client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(secret, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await secret.AddActivitiesAsync(newActivities);



                await this._user1.FollowFeedAsync(secret);


                activities = (await this._user1.GetActivitiesAsync(0, 5)).Results;
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
            var secret = _client.Feed("secret", System.Guid.NewGuid().ToString());

            //This initial unfollow is just to reset any existing follows or actvities, not a part of the test
            await this._user1.UnfollowFeedAsync(secret, false);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;

            if (activities.Count() == 0)
            {
                Activity[] newActivities = { new Activity("1", "test", "1"), new Activity("1", "test", "2"), new Activity("1", "test", "3"), new Activity("1", "test", "4"), new Activity("1", "test", "5") };
                var response = await secret.AddActivitiesAsync(newActivities);



                await this._user1.FollowFeedAsync(secret, 3);


                activities = (await this._user1.GetActivitiesAsync(0, 5)).Results;
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
            var newActivity = new Activity("1", "tweet", "1");
            var first = await _not5.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "run", "2");
            var second = await _not5.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "share", "3");
            var third = await _not5.AddActivityAsync(newActivity);

            var activities = (await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(2).WithMarker(ActivityMarker.Mark().AllRead()))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            activities = (await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(2).WithMarker(ActivityMarker.Mark().AllRead()))).Results;
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
        public async Task TestMarkReadByIds()
        {
            var newActivity = new Activity("1", "tweet", "1");
            var first = await _not5.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "run", "2");
            var second = await _not5.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "share", "3");
            var third = await _not5.AddActivityAsync(newActivity);

            var activities = (await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(2))).Results;

            var marker = ActivityMarker.Mark();
            foreach (var activity in activities)
            {
                marker = marker.Read(activity.Id);
            }

            var notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);

            activities = (await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(3).WithMarker(marker))).Results;

            activities = (await _not5.GetNotificationActivities(GetOptions.Default.WithLimit(3))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(3, activities.Count());

            notActivity = activities.First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(1).First();
            Assert.IsNotNull(notActivity);
            Assert.IsTrue(notActivity.IsRead);

            notActivity = activities.Skip(2).First();
            Assert.IsNotNull(notActivity);
            Assert.IsFalse(notActivity.IsRead);
        }

        [Test]
        public async Task TestMarkNotificationsRead()
        {
            var newActivity = new Activity("1", "tweet", "1");
            var first = await _not5.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "run", "2");
            var second = await _not5.AddActivityAsync(newActivity);

            newActivity = new Activity("1", "share", "3");
            var third = await _not5.AddActivityAsync(newActivity);



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
            var lonely = _client.Feed("flat", "lonely");
            var response = (await lonely.FollowersAsync()).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowersWithLimit()
        {
            var feed = _client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed1 = _client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed2 = _client.Feed("flat", System.Guid.NewGuid().ToString());

            // unfollow
            await feed1.UnfollowFeedAsync(feed);
            await feed2.UnfollowFeedAsync(feed);



            var response = (await feed.FollowersAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeedAsync(feed);
            await feed2.FollowFeedAsync(feed);



            response = (await feed.FollowersAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var first = response.First();
            Assert.AreEqual(first.FeedId, feed2.FeedId);
            Assert.AreEqual(first.TargetId, feed.FeedId);
            Assert.IsTrue(first.CreatedAt > DateTime.Now.AddDays(-1));
        }

        [Test]
        public async Task TestFollowingEmpty()
        {
            var lonely = _client.Feed("flat", "lonely");
            var response = (await lonely.FollowingAsync()).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestFollowingsWithLimit()
        {
            var feed = _client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed1 = _client.Feed("flat", System.Guid.NewGuid().ToString());
            var feed2 = _client.Feed("flat", System.Guid.NewGuid().ToString());

            // unfollow
            await feed1.UnfollowFeedAsync(feed);
            await feed1.UnfollowFeedAsync(feed2);



            var response = (await feed1.FollowingAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());

            await feed1.FollowFeedAsync(feed);
            await feed1.FollowFeedAsync(feed2);



            response = (await feed1.FollowingAsync(0, 2)).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());
            Assert.AreEqual(response.First().FeedId, feed1.FeedId);
            Assert.AreEqual(response.First().TargetId, feed2.FeedId);
        }

        [Test]
        public async Task TestDoIFollowEmpty()
        {
            var lonely = _client.Feed("flat", "lonely");
            var response = (await lonely.FollowingAsync(0, 10, new String[] { "flat:asocial" })).Results;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Count());
        }

        [Test]
        public async Task TestAggregate()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            var response = await _user1.AddActivityAsync(newActivity1);
            response = await _user1.AddActivityAsync(newActivity2);

            await _agg4.FollowFeedAsync(this._user1);

            var activities = (await this._agg4.GetAggregateActivitiesAsync()).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(2, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await _agg4.UnfollowFeedAsync(this._user1);
        }

        [Test]
        public async Task TestGetAggregate()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            await _user1.AddActivityAsync(newActivity1);
            await _user1.AddActivityAsync(newActivity2);

            await _agg4.FollowFeedAsync(this._user1);

            var result = await this._agg4.GetAggregateActivitiesAsync();
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

            await _agg4.UnfollowFeedAsync(this._user1);
        }

        [Test]
        public async Task TestMixedAggregate()
        {
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            var newActivity3 = new Activity("1", "other", "2");
            var response = await _user1.AddActivityAsync(newActivity1);
            response = await _user1.AddActivityAsync(newActivity2);
            response = await _user1.AddActivityAsync(newActivity3);



            await _agg4.FollowFeedAsync(this._user1);


            var activities = (await this._agg4.GetAggregateActivitiesAsync(null)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            var aggActivity = activities.First() as AggregateActivity;
            Assert.IsNotNull(aggActivity);
            Assert.AreEqual(1, aggActivity.Activities.Count);
            Assert.AreEqual(1, aggActivity.ActorCount);

            await _agg4.UnfollowFeedAsync(this._user1);
        }


        [Test]
        public async Task TestBatchFollow()
        {
            await _client.Batch.FollowManyAsync(new[]
            {
                new Follow(_user1, _flat3),
                new Follow(_user2, _flat3)
            });


            var newActivity = new Activity("1", "test", "1");
            var response = await this._flat3.AddActivityAsync(newActivity);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);

            activities = (await this._user2.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);
        }

        [Test]
        public async Task TestBatchFollowWithCopyLimit()
        {
            await _client.Batch.FollowManyAsync(new[]
            {
                new Follow(this._user1, this._flat3),
                new Follow(this._user2, this._flat3)
            }, 10);


            var newActivity = new Activity("1", "test", "1");
            var response = await this._flat3.AddActivityAsync(newActivity);

            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);

            activities = (await this._user2.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());
            Assert.AreEqual(response.Id, activities.First().Id);
            Assert.AreEqual("test", activities.First().Verb);
        }


        [Test]
        public async Task TestAddToMany()
        {
            var newActivity = new Activity("1", "test", "1");
            await _client.Batch.AddToManyAsync(newActivity, new[]
            {
                _user1, _user2
            });


            var activities = (await this._user1.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(newActivity.Actor, first.Actor);
            Assert.AreEqual(newActivity.Object, first.Object);
            Assert.AreEqual(newActivity.Verb, first.Verb);

            activities = (await this._user2.GetActivitiesAsync(0, 1)).Results;
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
            var newActivity1 = new Activity("1", "test", "1");
            var newActivity2 = new Activity("1", "test", "2");
            var newActivity3 = new Activity("1", "other", "2");
            var addedActivities = new List<Activity>();

            var response = await this._user1.AddActivityAsync(newActivity1);
            addedActivities.Add(response);
            response = await this._user2.AddActivityAsync(newActivity2);
            addedActivities.Add(response);
            response = await this._flat3.AddActivityAsync(newActivity3);
            addedActivities.Add(response);


            var activities = (await _client.Batch.GetActivitiesByIdAsync(addedActivities.Select(a => a.Id))).Results;
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
            var newActivity1 = new Activity("1", "test", "1")
            {
                ForeignId = "fid-test-1",
                Time = DateTime.Parse("2000-08-16T16:32:32")
            };

            var newActivity2 = new Activity("1", "test", "2")
            {
                ForeignId = "fid-test-2",
                Time = DateTime.Parse("2000-08-17T16:32:32")
            };

            var newActivity3 = new Activity("1", "other", "2")
            {
                ForeignId = "fid-other-1",
                Time = DateTime.Parse("2000-08-19T16:32:32")
            };

            var addedActivities = new List<Activity>();

            var response = await this._user1.AddActivityAsync(newActivity1);
            addedActivities.Add(response);
            response = await this._user2.AddActivityAsync(newActivity2);
            addedActivities.Add(response);
            response = await this._flat3.AddActivityAsync(newActivity3);
            addedActivities.Add(response);


            var activities = (await _client.Batch.GetActivitiesByForeignIdAsync(
                addedActivities.Select(a => new ForeignIdTime(a.ForeignId, a.Time.Value)))).Results;
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
        public async Task TestGetEnrichedFlatActivitiesByID()
        {
            var userId = System.Guid.NewGuid().ToString();
            const string userName = "user name";
            var user = await _client.Users.AddAsync(userId, new Dictionary<string, object> { ["name"] = userName });
            var newActivity1 = new Activity(user.Ref(), "test", "1");
            var newActivity2 = new Activity(user.Ref(), "test", "2");
            var newActivity3 = new Activity(user.Ref(), "other", "2");
            var addedActivities = new List<Activity>();

            var response = await this._user1.AddActivityAsync(newActivity1);
            addedActivities.Add(response);
            response = await this._user2.AddActivityAsync(newActivity2);
            addedActivities.Add(response);
            response = await this._flat3.AddActivityAsync(newActivity3);
            addedActivities.Add(response);

            var activities = (await _client.Batch.GetEnrichedActivitiesAsync(addedActivities.Select(a => a.Id))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(addedActivities.Count, activities.Count());

            activities.ForEach(a =>
            {
                var found = addedActivities.Find(x => x.Id == a.Id);
                Assert.NotNull(found);

                // Assert.IsTrue(a.Actor.IsEnriched);
                // Assert.AreEqual(userId, a.Actor.Enriched.GetData<string>("id"));
                // var userData = a.Actor.Enriched.GetData<Dictionary<string, object>>("data");
                // Assert.IsNotNull(userData);
                // Assert.IsTrue(userData.ContainsKey("name"));
                // Assert.AreEqual(userName, userData["name"]);

                // Assert.IsFalse(a.Object.IsEnriched);
                // Assert.AreEqual(found.Object, a.Object.Raw);

                // Assert.IsFalse(a.Verb.IsEnriched);
                // Assert.AreEqual(found.Verb, a.Verb.Raw);
            });
        }

        [Test]
        public async Task TestGetEnrichedFlatActivitiesByIDWithReactions()
        {
            var userId = System.Guid.NewGuid().ToString();
            var user = await _client.Users.AddAsync(userId);
            var newActivity = new Activity(user.Ref(), "test", "1");
            newActivity = await this._user1.AddActivityAsync(newActivity);

            await _client.Reactions.AddAsync("upvote", newActivity.Id, user.Id, new Dictionary<string, object> { ["reactionProp"] = "reactionPropValue" });

            var activities = (await _client.Batch.GetEnrichedActivitiesAsync(
                new[] { newActivity.Id },
                new GetOptions().WithReaction(ReactionOption.With().Counts().Recent()))).Results;

            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var enrichedActivity = activities.Single();
            Assert.NotNull(enrichedActivity);

            // Assert.IsTrue(enrichedActivity.Actor.IsEnriched);
            // Assert.AreEqual(userId, enrichedActivity.Actor.Enriched.GetData<string>("id"));

            Assert.IsNotNull(enrichedActivity.ReactionCounts);
            Assert.AreEqual(1, enrichedActivity.ReactionCounts.Count);
            Assert.AreEqual("upvote", enrichedActivity.ReactionCounts.Keys.Single());
            Assert.AreEqual(1, enrichedActivity.ReactionCounts["upvote"]);

            Assert.IsNotNull(enrichedActivity.LatestReactions);
            Assert.AreEqual(1, enrichedActivity.LatestReactions.Count);
            Assert.AreEqual("upvote", enrichedActivity.LatestReactions.Keys.Single());
            // Assert.AreEqual(1, enrichedActivity.LatestReactions["upvote"].Count());

            var enrichedReaction = enrichedActivity.LatestReactions["upvote"];
            Assert.IsNotNull(enrichedReaction.First().Data);
            Assert.AreEqual("reactionPropValue", enrichedReaction.First().Data["reactionProp"]);
        }

        [Test]
        public void TestCollectionsUpsert()
        {
            var data = new CollectionObject(System.Guid.NewGuid().ToString());
            data.SetData("hobbies", new List<string> { "eating", "coding" });

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Collections.UpsertAsync("people", data);
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
                await _client.Collections.UpsertManyAsync("people", data);
            });
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

            await _client.Collections.UpsertManyAsync("people", data);

            var result = (await _client.Collections.SelectAsync("people", id1));

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

            await _client.Collections.UpsertManyAsync("people", data);

            var results = (await _client.Collections.SelectMany("people", new[] { id1, id2 })).Response.Data;

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
        public async Task TestCollectionsDelete()
        {
            string id1 = System.Guid.NewGuid().ToString(),
            id2 = System.Guid.NewGuid().ToString();
            var data1 = new CollectionObject(id1);
            data1.SetData("hobbies", new List<string> { "eating", "coding" });
            var data2 = new CollectionObject(id2);
            data2.SetData("vacation", new List<string> { "Spain", "Iceland" });

            var data = new List<CollectionObject> { data1, data2 };

            await _client.Collections.UpsertManyAsync("people", data);

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Collections.DeleteAsync("people", id2);
            });

            var results = (await _client.Collections.SelectMany("people", new string[] { id1, id2 })).Response.Data;

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

            await _client.Collections.UpsertManyAsync("people", data);

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Collections.DeleteManyAsync("people", new string[] { id1, id2 });
            });

            var results = (await _client.Collections.SelectMany("people", new string[] { id1, id2 })).Response.Data;

            Assert.NotNull(results);
            Assert.AreEqual(0, results.CountOrFallback());
        }

        [Test]
        public async Task TestCollectionsCRUD()
        {
            var colData = new Dictionary<string, object>();
            colData.Add("field", "value");
            colData.Add("flag", true);

            //ADD
            var collectionObject = await _client.Collections.AddAsync("col_test_crud", colData);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.Id));
            Assert.AreEqual("value", collectionObject.GetData<string>("field"));
            Assert.AreEqual(true, collectionObject.GetData<bool>("flag"));

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var o = await _client.Collections.AddAsync("col_test_crud", colData, collectionObject.Id);
            });

            //GET
            collectionObject = await _client.Collections.GetAsync("col_test_crud", collectionObject.Id);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.Id));
            Assert.AreEqual("value", collectionObject.GetData<string>("field"));
            Assert.AreEqual(true, collectionObject.GetData<bool>("flag"));

            //UPDATE
            var newData = new Dictionary<string, object>();
            newData.Add("new", "stuff");
            newData.Add("arr", new[] { "a", "b" });
            collectionObject = await _client.Collections.UpdateAsync("col_test_crud", collectionObject.Id, newData);

            Assert.NotNull(collectionObject);
            Assert.False(string.IsNullOrEmpty(collectionObject.Id));
            Assert.AreEqual("stuff", collectionObject.GetData<string>("new"));
            Assert.AreEqual(new string[] { "a", "b" }, collectionObject.GetData<string[]>("arr"));

            //DELETE
            await _client.Collections.DeleteAsync("col_test_crud", collectionObject.Id);

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var o = await _client.Collections.GetAsync("col_test_crud", collectionObject.Id);
            });
        }

        [Test]
        public async Task TestActivityPartialUpdateByID()
        {
            var act = new Activity("upd", "test", "1")
            {
                ForeignId = System.Guid.NewGuid().ToString(),
                Time = DateTime.UtcNow
            };
            act.SetData("custom_thing", "12345");
            act.SetData("custom_thing2", "foobar");
            act.SetData("custom_thing3", "some thing");

            var insertedAct = await this._user1.AddActivityAsync(act);
            Assert.IsNotNull(insertedAct);
            Assert.AreEqual("12345", insertedAct.GetData<string>("custom_thing"));
            Assert.AreEqual("foobar", insertedAct.GetData<string>("custom_thing2"));
            Assert.AreEqual("some thing", insertedAct.GetData<string>("custom_thing3"));

            var set = new Dictionary<string, object>();
            set.Add("custom_thing", "abcdef");

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.ActivityPartialUpdateAsync(insertedAct.Id, null, set);
            });

            var updatedAct = (await _client.Batch.GetActivitiesByIdAsync(new[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual("abcdef", updatedAct.GetData<string>("custom_thing"));

            var unset = new string[] { "custom_thing2" };

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.ActivityPartialUpdateAsync(insertedAct.Id, null, null, unset);
            });

            updatedAct = (await _client.Batch.GetActivitiesByIdAsync(new string[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing2"));

            set["custom_thing"] = "zyx";
            unset[0] = "custom_thing3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.ActivityPartialUpdateAsync(insertedAct.Id, null, set, unset);
            });

            updatedAct = (await _client.Batch.GetActivitiesByIdAsync(new string[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing3"));
            Assert.AreEqual("zyx", updatedAct.GetData<string>("custom_thing"));
        }

        [Test]
        public async Task TestActivityPartialUpdateByForeignIDTime()
        {
            var fidTime = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);
            var act = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime.ForeignId,
                Time = fidTime.Time
            };
            act.SetData("custom_thing", "12345");
            act.SetData("custom_thing2", "foobar");
            act.SetData("custom_thing3", "some thing");

            var insertedAct = await this._user1.AddActivityAsync(act);
            Assert.IsNotNull(insertedAct);
            Assert.AreEqual("12345", insertedAct.GetData<string>("custom_thing"));
            Assert.AreEqual("foobar", insertedAct.GetData<string>("custom_thing2"));
            Assert.AreEqual("some thing", insertedAct.GetData<string>("custom_thing3"));

            var set = new Dictionary<string, object>
            {
                { "custom_thing", "abcdef" }
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.ActivityPartialUpdateAsync(null, fidTime, set);
            });

            var updatedAct = (await _client.Batch.GetActivitiesByIdAsync(new string[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual("abcdef", updatedAct.GetData<string>("custom_thing"));

            var unset = new string[] { "custom_thing2" };

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.ActivityPartialUpdateAsync(null, fidTime, null, unset);
            });

            updatedAct = (await _client.Batch.GetActivitiesByIdAsync(new string[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing2"));

            set["custom_thing"] = "zyx";
            unset[0] = "custom_thing3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.ActivityPartialUpdateAsync(null, fidTime, set, unset);
            });

            updatedAct = (await _client.Batch.GetActivitiesByIdAsync(new string[] { insertedAct.Id })).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.IsNull(updatedAct.GetData<string>("custom_thing3"));
            Assert.AreEqual("zyx", updatedAct.GetData<string>("custom_thing"));
        }

        [Test]
        public async Task TestUpdateToTargets()
        {
            var fidTime = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);

            var targets = new List<string>()
            {
                "flat:" + Guid.NewGuid().ToString(),
                "user:" + Guid.NewGuid().ToString(),
            };

            var act = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime.ForeignId,
                Time = fidTime.Time,
                To = targets
            };

            var insertedAct = await this._user1.AddActivityAsync(act);
            Assert.AreEqual(2, insertedAct.To.Count);

            //add 1
            var add = "user:" + Guid.NewGuid().ToString();
            var updateResp = await this._user1.UpdateActivityToTargetsAsync(fidTime, new string[] { add });
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(1, updateResp.Added.Count);
            Assert.AreEqual(add, updateResp.Added[0]);
            Assert.AreEqual(3, updateResp.Activity.To.Count);
            Assert.IsNotNull(updateResp.Activity.To.ToList().Find(t => t == add));

            var updatedAct = (await this._user1.GetActivitiesAsync(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(3, updatedAct.To.Count);
            Assert.IsNotNull(updatedAct.To.ToList().Find(t => t == add));

            //remove 1
            var remove = targets[0];
            updateResp = await this._user1.UpdateActivityToTargetsAsync(fidTime, null, null, new string[] { remove });
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(1, updateResp.Removed.Count);
            Assert.AreEqual(remove, updateResp.Removed[0]);
            Assert.AreEqual(2, updateResp.Activity.To.Count);
            Assert.IsNull(updateResp.Activity.To.ToList().Find(t => t == remove));

            updatedAct = (await this._user1.GetActivitiesAsync(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(2, updatedAct.To.Count);
            Assert.IsNull(updatedAct.To.ToList().Find(t => t == remove));

            //new ones
            var newOnes = new List<string>()
            {
                "flat:" + Guid.NewGuid().ToString(),
                "user:" + Guid.NewGuid().ToString(),
            };
            updateResp = await this._user1.UpdateActivityToTargetsAsync(fidTime, null, newOnes);
            Assert.AreEqual(insertedAct.Id, updateResp.Activity.Id);
            Assert.AreEqual(2, updateResp.Activity.To.Count);
            Assert.AreEqual(2, updateResp.Added.Count);
            Assert.AreEqual(2, updateResp.Added.ToList().FindAll(t => newOnes.Contains(t)).Count);
            updatedAct = (await this._user1.GetActivitiesAsync(0, 1, FeedFilter.Where().IdLessThanEqual(insertedAct.Id))).Results.FirstOrDefault();
            Assert.NotNull(updatedAct);
            Assert.AreEqual(2, updatedAct.To.Count);
            Assert.AreEqual(2, updatedAct.To.ToList().FindAll(t => newOnes.Contains(t)).Count);
        }

        [Test]
        public async Task TestBatchPartialUpdate()
        {
            var fidTime1 = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow);
            var act1 = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime1.ForeignId,
                Time = fidTime1.Time
            };
            act1.SetData("custom_thing", "12345");
            act1.SetData("custom_thing2", "foobar");
            act1.SetData("custom_thing3", "some thing");
            var fidTime2 = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-3));
            var act2 = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime2.ForeignId,
                Time = fidTime2.Time
            };
            act2.SetData("custom_flag", "val1");
            act2.SetData("custom_flag2", "val2");
            act2.SetData("custom_flag3", "val3");

            var fidTime3 = new ForeignIdTime(System.Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-6));
            var act3 = new Activity("upd", "test", "1")
            {
                ForeignId = fidTime3.ForeignId,
                Time = fidTime3.Time
            };
            var customData = new Dictionary<string, string>()
            {
                {"name", "BOB"},
                {"address", "90210"},
                {"email", "bob@bobobo.com"},
            };
            act3.SetData("details", customData);

            var response = (await this._user1.AddActivitiesAsync(new[] { act1, act2, act3 })).Activities;
            var insertedActs = response.ToArray();

            //FID TIME
            var upd1 = new ActivityPartialUpdateRequestObject()
            {
                ForeignId = fidTime1.ForeignId,
                Time = fidTime1.Time,
                Unset = new string[] { "custom_thing3" }
            };

            var set = new Dictionary<string, object>
            {
                {"details.address", "nowhere"},
            };

            var upd2 = new ActivityPartialUpdateRequestObject()
            {
                ForeignId = fidTime3.ForeignId,
                Time = fidTime3.Time,
                Set = set
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Batch.ActivitiesPartialUpdateAsync(new[] { upd1, upd2 });
            });

            var updatedActs = (await this._user1.GetActivitiesAsync()).Results.ToArray();

            Assert.IsNull(updatedActs[0].GetData<string>("custom_thing3"));
            var extraData = updatedActs[2].GetData<Dictionary<string, string>>("details");
            Assert.AreEqual("nowhere", extraData["address"]);

            //ID
            set.Clear();
            set.Add("custom_flag2", "foobar");
            upd1 = new ActivityPartialUpdateRequestObject()
            {
                Id = insertedActs[1].Id,
                Set = set
            };
            upd2 = new ActivityPartialUpdateRequestObject()
            {
                Id = insertedActs[2].Id,
                Unset = new string[] { "details.name" }
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Batch.ActivitiesPartialUpdateAsync(new[] { upd1, upd2 });
            });

            updatedActs = (await this._user1.GetActivitiesAsync()).Results.ToArray();

            Assert.AreEqual("foobar", updatedActs[1].GetData<string>("custom_flag2"));
            extraData = updatedActs[2].GetData<Dictionary<string, string>>("details");
            Assert.False(extraData.ContainsKey("name"));
        }

        [Test]
        public async Task TestBatchUpdateActivity()
        {
            var activity = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };
            activity.SetData("custom", "field");
            var insertedActivity = await this._user1.AddActivityAsync(activity);

            activity.Target = "timmy";
            activity.SetData("custom", "data");
            activity.SetData("another", "thing");

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Batch.UpdateActivity(activity);
            });

            var updatedActivity = (await this._user1.GetActivitiesAsync(0, 1)).Results.FirstOrDefault();
            Assert.NotNull(updatedActivity);
            Assert.AreEqual(insertedActivity.Id, updatedActivity.Id);
            Assert.AreEqual(activity.Target, updatedActivity.Target);
            Assert.AreEqual(activity.GetData<string>("custom"), updatedActivity.GetData<string>("custom"));
            Assert.AreEqual(activity.GetData<string>("another"), updatedActivity.GetData<string>("another"));
        }

        [Test]
        public async Task TestBatchUpdateActivities()
        {
            var activity = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };
            activity.SetData("custom", "field");
            var activity2 = new Activity("user:123", "posts", "selfie")
            {
                ForeignId = "selfie:2",
                Time = DateTime.UtcNow,
            };

            var insertedActivity = await this._user1.AddActivityAsync(activity);
            var insertedActivity2 = await this._flat3.AddActivityAsync(activity2);

            activity.SetData("custom", "data");
            activity.Target = null;
            activity2.SetData("new-stuff", new int[] { 3, 2, 1 });
            activity2.Actor = "user:3";

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Batch.UpdateActivitiesAsync(new[] { activity, activity2 });
            });

            var updatedActivity = (await this._user1.GetActivitiesAsync(0, 1)).Results.FirstOrDefault();
            Assert.NotNull(updatedActivity);
            Assert.AreEqual(insertedActivity.Id, updatedActivity.Id);
            Assert.AreEqual(string.Empty, updatedActivity.Target);
            Assert.AreEqual(activity.GetData<string>("custom"), updatedActivity.GetData<string>("custom"));

            var updatedActivity2 = (await this._flat3.GetActivitiesAsync(0, 1)).Results.FirstOrDefault();
            Assert.NotNull(updatedActivity2);
            Assert.AreEqual(insertedActivity2.Id, updatedActivity2.Id);
            Assert.AreEqual(activity2.Actor, updatedActivity2.Actor);
            Assert.AreEqual(activity2.GetData<int[]>("custom"), updatedActivity2.GetData<int[]>("custom"));
        }

        [Test]
        public async Task TestReactions()
        {
            var a = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };

            var activity = await this._user1.AddActivityAsync(a);

            var data = new Dictionary<string, object>()
            {
                {"field", "value"},
                {"number", 2}
            };

            Reaction r = null;
            // Add reaction
            Assert.DoesNotThrowAsync(async () =>
            {
                r = await _client.Reactions.AddAsync("like", activity.Id, "bobby", data);
            });

            Assert.NotNull(r);
            Assert.AreEqual(r.ActivityId, activity.Id);
            Assert.AreEqual(r.Kind, "like");
            Assert.AreEqual(r.UserId, "bobby");
            Assert.AreEqual(r.Data, data);
            Assert.True(r.CreatedAt.HasValue);
            Assert.True(r.UpdatedAt.HasValue);
            Assert.IsNotEmpty(r.Id);

            // get reaction
            Reaction r2 = null;
            Assert.DoesNotThrowAsync(async () =>
            {
                r2 = await _client.Reactions.GetAsync(r.Id);
            });

            Assert.NotNull(r2);
            Assert.AreEqual(r2.ActivityId, r.ActivityId);
            Assert.AreEqual(r2.Kind, "like");
            Assert.AreEqual(r2.UserId, "bobby");
            Assert.AreEqual(r2.Data, r.Data);
            Assert.AreEqual(r2.Id, r.Id);

            // Update reaction
            data["number"] = 321;
            data["new"] = "field";
            data.Remove("field");

            var beforeTime = r.UpdatedAt.Value;
            Assert.DoesNotThrowAsync(async () =>
            {
                r2 = await _client.Reactions.UpdateAsync(r.Id, data);
            });
            Assert.NotNull(r2);
            Assert.False(r2.Data.ContainsKey("field"));
            object n;
            Assert.True(r2.Data.TryGetValue("number", out n));
            Assert.AreEqual((Int64)n, 321);
            Assert.True(r2.Data.ContainsKey("new"));

            // Add children
            var c1 = await _client.Reactions.AddChildAsync(r, "upvote", "tommy");
            var c2 = await _client.Reactions.AddChildAsync(r, "downvote", "timmy");
            var c3 = await _client.Reactions.AddChildAsync(r, "upvote", "jimmy");

            var parent = await _client.Reactions.GetAsync(r.Id);

            Assert.AreEqual(parent.ChildrenCounters["upvote"], 2);
            Assert.AreEqual(parent.ChildrenCounters["downvote"], 1);

            Assert.IsTrue(parent.LatestChildren["upvote"].Select(x => x.Id).Contains(c1.Id));
            Assert.IsTrue(parent.LatestChildren["upvote"].Select(x => x.Id).Contains(c3.Id));
            Assert.IsTrue(parent.LatestChildren["downvote"].Select(x => x.Id).Contains(c2.Id));

            // Delete reaction

            Assert.DoesNotThrowAsync(async () =>
            {
                await _client.Reactions.DeleteAsync(r.Id);
            });

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var r3 = await _client.Reactions.GetAsync(r.Id);
            });
        }

        [Test]
        public async Task TestReactionPagination()
        {
            var a = new Activity("user:1", "like", "cake")
            {
                ForeignId = "cake:1",
                Time = DateTime.UtcNow,
                Target = "johnny"
            };

            var activity = await this._user1.AddActivityAsync(a);

            a.Time = DateTime.UtcNow;
            a.ForeignId = "cake:123";
            var activity2 = await this._user1.AddActivityAsync(a);

            var data = new Dictionary<string, object>()
            {
                {"field", "value"},
                {"number", 2}
            };

            var userId = Guid.NewGuid().ToString();

            var r1 = await _client.Reactions.AddAsync("like", activity.Id, userId, data);
            var r2 = await _client.Reactions.AddAsync("comment", activity.Id, userId, data);
            var r3 = await _client.Reactions.AddAsync("like", activity.Id, "bob", data);

            var r4 = await _client.Reactions.AddChildAsync(r3, "upvote", "tom", data);
            var r5 = await _client.Reactions.AddChildAsync(r3, "upvote", "mary", data);

            // activity id
            var filter = ReactionFiltering.Default;
            var pagination = ReactionPagination.By.ActivityId(activity.Id).Kind("like");

            var reactionsByActivity = await _client.Reactions.FilterAsync(filter, pagination);
            Assert.AreEqual(2, reactionsByActivity.Count());

            var r = (List<Reaction>)reactionsByActivity;
            var actual = r.Find(x => x.Id == r1.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r1.Id, actual.Id);
            Assert.AreEqual(r1.Kind, actual.Kind);
            Assert.AreEqual(r1.ActivityId, actual.ActivityId);

            actual = r.Find(x => x.Id == r3.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r3.Id, actual.Id);
            Assert.AreEqual(r3.Kind, actual.Kind);
            Assert.AreEqual(r3.ActivityId, actual.ActivityId);

            //with limit
            reactionsByActivity = await _client.Reactions.FilterAsync(filter.WithLimit(1), pagination);
            Assert.AreEqual(1, reactionsByActivity.Count());

            //with data
            var reactionsByActivityWithData = await _client.Reactions.FilterWithActivityAsync(filter.WithLimit(1), pagination);
            Assert.AreEqual(1, reactionsByActivity.Count());
            Assert.AreEqual(data, reactionsByActivity.FirstOrDefault().Data);

            // user id
            filter = ReactionFiltering.Default;
            pagination = ReactionPagination.By.UserId(userId);

            var reactionsByUser = await _client.Reactions.FilterAsync(filter, pagination);
            Assert.AreEqual(2, reactionsByUser.Count());

            r = (List<Reaction>)reactionsByUser;
            actual = r.Find(x => x.Id == r1.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r1.Id, actual.Id);
            Assert.AreEqual(r1.Kind, actual.Kind);
            Assert.AreEqual(r1.ActivityId, actual.ActivityId);

            actual = r.Find(x => x.Id == r2.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r2.Id, actual.Id);
            Assert.AreEqual(r2.Kind, actual.Kind);
            Assert.AreEqual(r2.ActivityId, actual.ActivityId);

            // reaction id
            filter = ReactionFiltering.Default;
            pagination = ReactionPagination.By.Kind("upvote").ReactionId(r3.Id);

            var reactionsByParent = await _client.Reactions.FilterAsync(filter, pagination);
            Assert.AreEqual(2, reactionsByParent.Count());

            r = (List<Reaction>)reactionsByParent;
            actual = r.Find(x => x.Id == r4.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r4.Id, actual.Id);
            Assert.AreEqual(r4.Kind, actual.Kind);
            Assert.AreEqual(r4.ActivityId, actual.ActivityId);
            Assert.AreEqual(r4.UserId, actual.UserId);

            actual = r.Find(x => x.Id == r5.Id);

            Assert.NotNull(actual);
            Assert.AreEqual(r5.Id, actual.Id);
            Assert.AreEqual(r5.Kind, actual.Kind);
            Assert.AreEqual(r5.ActivityId, actual.ActivityId);
            Assert.AreEqual(r5.UserId, actual.UserId);
        }

        [Test]
        public async Task TestUsers()
        {
            //Create user
            var userId = Guid.NewGuid().ToString();
            var userData = new Dictionary<string, object>()
            {
                {"field", "value"},
                {"is_admin", true},
            };

            User u = null;
            Assert.DoesNotThrowAsync(async () =>
            {
                u = await _client.Users.AddAsync(userId, userData);
            });

            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(userData, u.Data);

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                u = await _client.Users.AddAsync(userId, userData);
            });

            var newUserData = new Dictionary<string, object>()
            {
                {"field", "othervalue"},
            };
            Assert.DoesNotThrowAsync(async () =>
            {
                u = await _client.Users.AddAsync(userId, newUserData, true);
            });
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(userData, u.Data);

            //Get user
            u = await _client.Users.GetAsync(userId);
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(userData, u.Data);

            // Update user
            u = await _client.Users.UpdateAsync(userId, newUserData);
            Assert.NotNull(u);
            Assert.NotNull(u.CreatedAt);
            Assert.NotNull(u.UpdatedAt);
            Assert.AreEqual(userId, u.Id);
            Assert.AreEqual(newUserData, u.Data);

            //Delete user
            await _client.Users.DeleteAsync(userId);

            Assert.ThrowsAsync<Stream.StreamException>(async () =>
            {
                var x = await _client.Users.GetAsync(userId);
            });
        }

        [Test]
        public async Task TestEnrich_Collection()
        {
            var c = new CollectionObject(Guid.NewGuid().ToString());
            c.SetData("field", "testing_value");
            await _client.Collections.UpsertAsync("items", c);
            var cRef = _client.Collections.Ref("items", c);

            var a = new Activity("actor-1", "add", cRef);
            await this._user1.AddActivityAsync(a);

            var plain = await this._user1.GetFlatActivitiesAsync();
            Assert.AreEqual(cRef, plain.Results.First().Object);

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync();
            Assert.AreEqual(1, enriched.Results.Count());

            var act = enriched.Results.First();
            Assert.NotNull(act.Actor);
            Assert.AreEqual("actor-1", act.Actor.Id);
            Assert.AreEqual(c.Id, act.Object.Id);
            var dataJobject = act.Object.GetData<Dictionary<string, object>>("data")["data"] as JObject;
            Assert.AreEqual("testing_value", dataJobject["field"].ToString());
        }

        [Test]
        public async Task TestEnrich_User()
        {
            var userData = new Dictionary<string, object>()
            {
                {"is_admin", true},
                {"nickname","bobby"}
            };
            var u = await _client.Users.AddAsync(Guid.NewGuid().ToString(), userData);
            var uRef = u.Ref();

            var a = new Activity(uRef, "add", "post");
            await this._user1.AddActivityAsync(a);

            var plain = await this._user1.GetFlatActivitiesAsync();
            Assert.AreEqual(uRef, plain.Results.First().Actor);

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync();

            Assert.AreEqual(1, enriched.Results.Count());

            var act = enriched.Results.First();
            // Assert.IsFalse(act.Object.IsEnriched);
            // Assert.AreEqual("post", act.Object.Raw);
            // Assert.IsTrue(act.Actor.IsEnriched);
            // Assert.AreEqual(u.ID, act.Actor.Enriched.GetData<string>("id"));
            // Assert.AreEqual(userData, act.Actor.Enriched.GetData<Dictionary<string, object>>("data"));
        }

        [Test]
        public async Task TestEnrich_OwnReaction()
        {
            var a = new Activity("johhny", "add", "post");
            var act = await this._user1.AddActivityAsync(a);
            var reaction = await _client.Reactions.AddAsync("like", act.Id, "johhny");

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Own()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.OwnReactions.ContainsKey(reaction.Kind));

            Assert.AreEqual(reaction.Id, enrichedAct.OwnReactions[reaction.Kind].First().Id);
            Assert.AreEqual(reaction.Kind, enrichedAct.OwnReactions[reaction.Kind].First().Kind);
            Assert.AreEqual(reaction.UserId, enrichedAct.OwnReactions[reaction.Kind].First().UserId);
        }

        [Test]
        public async Task TestEnrich_User_InReaction()
        {
            var userData = new Dictionary<string, object>()
            {
                {"is_admin", true},
                {"nickname","bobby"}
            };
            var u = await _client.Users.AddAsync(Guid.NewGuid().ToString(), userData);
            var uRef = u.Ref();

            var a = new Activity(uRef, "add", "post");
            var act = await this._user1.AddActivityAsync(a);

            var reaction = await _client.Reactions.AddAsync("like", act.Id, u.Id);

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Own()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.OwnReactions.ContainsKey(reaction.Kind));
            var ownReaction = enrichedAct.OwnReactions[reaction.Kind];

            Assert.AreEqual(reaction.Id, ownReaction.First().Id);
            Assert.AreEqual(reaction.Kind, ownReaction.First().Kind);
            Assert.AreEqual(reaction.UserId, ownReaction.First().UserId);
            Assert.AreEqual(reaction.UserId, ownReaction.First().User.Id);
            Assert.AreEqual("bobby", ownReaction.First().User.GetData<Dictionary<string, object>>("data")["nickname"] as string);
            Assert.AreEqual(true, (bool)ownReaction.First().User.GetData<Dictionary<string, object>>("data")["is_admin"]);
        }

        [Test]
        public async Task TestEnrich_LatestReactions()
        {
            var a = new Activity("johhny", "add", "post");
            var act = await this._user1.AddActivityAsync(a);
            var reaction = await _client.Reactions.AddAsync("like", act.Id, "johhny");

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.Id, enrichedAct.LatestReactions[reaction.Kind].First().Id);
            Assert.AreEqual(reaction.Kind, enrichedAct.LatestReactions[reaction.Kind].First().Kind);
            Assert.AreEqual(reaction.UserId, enrichedAct.LatestReactions[reaction.Kind].First().UserId);

            var comment = await _client.Reactions.AddAsync("comment", act.Id, "bobby");
            await _client.Reactions.AddAsync("comment", act.Id, "tony");
            await _client.Reactions.AddAsync("comment", act.Id, "rupert");

            enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent().KindFilter("comment").KindFilter("upvote")));

            Assert.AreEqual(1, enriched.Results.Count());

            enrichedAct = enriched.Results.First();

            Assert.False(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.True(enrichedAct.LatestReactions.ContainsKey(comment.Kind));

            enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent().RecentLimit(1)));

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
            var a = new Activity("johhny", "add", "post");
            var act = await this._user1.AddActivityAsync(a);
            var reactionLike = await _client.Reactions.AddAsync("like", act.Id, "johhny");
            var reactionComment = await _client.Reactions.AddAsync("comment", act.Id, "johhny");
            var reactionLike2 = await _client.Reactions.AddAsync("like", act.Id, "timmeh");

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Counts()));

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
            var a = new Activity("johhny", "add", "post");
            var act = await this._user1.AddActivityAsync(a);
            var reaction = await _client.Reactions.AddAsync("like", act.Id, "johhny");

            var enriched = await this._user1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent().Own().Counts()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));

            Assert.AreEqual(reaction.Id, enrichedAct.LatestReactions[reaction.Kind].First().Id);
            Assert.AreEqual(reaction.Kind, enrichedAct.LatestReactions[reaction.Kind].First().Kind);
            Assert.AreEqual(reaction.UserId, enrichedAct.LatestReactions[reaction.Kind].First().UserId);

            Assert.True(enrichedAct.OwnReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.Id, enrichedAct.OwnReactions[reaction.Kind].First().Id);
            Assert.AreEqual(reaction.Kind, enrichedAct.OwnReactions[reaction.Kind].First().Kind);
            Assert.AreEqual(reaction.UserId, enrichedAct.OwnReactions[reaction.Kind].First().UserId);

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

            var response = await _p.Personalization.GetAsync("etoro_newsfeed", new Dictionary<string, object>()
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

        [Test]
        public async Task TestUpload()
        {
            Upload upload;

            using (var fs = File.OpenRead("../../../helloworld.txt"))
            {
                upload = await _client.Files.UploadAsync(fs, "helloworld.txt");
                Assert.IsNotEmpty(upload.File);

                await _client.Files.DeleteAsync(upload.File);
            }
            using (var fs = File.OpenRead("../../../helloworld.txt"))
            {
                upload = await _client.Files.UploadAsync(fs, "helloworld.txt", "text/plain");
                Assert.IsNotEmpty(upload.File);

                await _client.Files.DeleteAsync(upload.File);
            }

            using (var fs = File.OpenRead(@"../../../helloworld.jpg"))
            {
                upload = await _client.Images.UploadAsync(fs, "helloworld.jpg", "image/jpeg");
                Assert.IsNotEmpty(upload.File);

                await _client.Images.DeleteAsync(upload.File);
            }
        }

        [Test]
        public async Task TestOG()
        {
            var og = await _client.OgAsync("https://getstream.io/blog/try-out-the-stream-api-with-postman");

            Assert.IsNotEmpty(og.Type);
            Assert.IsNotEmpty(og.Title);
            Assert.IsNotEmpty(og.Description);
            Assert.IsNotEmpty(og.Url);
            Assert.IsNotEmpty(og.Favicon);
            Assert.IsNotEmpty(og.Images);
            Assert.IsNotEmpty(og.Images[0].Image);
        }

        [Test]
        public async Task TestFollowStats()
        {
            var f1 = _client.Feed("user", System.Guid.NewGuid().ToString());
            var f2 = _client.Feed("user", System.Guid.NewGuid().ToString());
            await f1.FollowFeedAsync(f2);

            var stats = (await f1.FollowStatsAsync(null, new string[] { "timeline" })).Results;
            Assert.AreEqual(stats.Followers.Count, 0);
            Assert.AreEqual(stats.Followers.Feed, f1.FeedId);
            Assert.AreEqual(stats.Following.Count, 0);
            Assert.AreEqual(stats.Following.Feed, f1.FeedId);

            stats = (await f1.FollowStatsAsync(null, new string[] { "user" })).Results;
            Assert.AreEqual(stats.Followers.Count, 0);
            Assert.AreEqual(stats.Followers.Feed, f1.FeedId);
            Assert.AreEqual(stats.Following.Count, 1);
            Assert.AreEqual(stats.Following.Feed, f1.FeedId);
        }
    }
}
