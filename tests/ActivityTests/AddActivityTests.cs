using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class AddActivityTests : TestBase
    {
        [Test]
        public async Task TestAddActivities()
        {
            var newActivities = new[]
            {
                new Activity("multi1", "test", "1"),
                new Activity("multi2", "test", "2"),
            };

            var response = (await this.UserFeed.AddActivitiesAsync(newActivities)).Activities;
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 2)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(2, activities.Count());

            Assert.AreEqual(response.Skip(1).First().Id, activities.First().Id);
            Assert.AreEqual(response.First().Id, activities.Skip(1).First().Id);
        }

        [Test]
        public async Task TestAddActivity()
        {
            var newActivity = new Activity("1", "test", "1");
            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
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
            var newActivity = new Activity("1", "test", "1") { Time = now };

            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
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
                ContractResolver = new DefaultContractResolver { NamingStrategy = new KebabCaseNamingStrategy() },
            });
            newActivity.SetData(new Dictionary<string, object>
            {
                { "dictkey", "dictvalue" },
            });
            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<string[]>("complex");
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
            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<string>("complex");
            Assert.IsNotNull(complex);
            Assert.AreEqual("string", complex);
        }

        [Test]
        public async Task TestAddActivityWithDictionary()
        {
            var dict = new Dictionary<string, string>()
            {
                { "test1", "shawn" },
                { "test2", "wedge" },
            };

            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData("complex", dict);

            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(response.Id, first.Id);

            var complex = first.GetData<Dictionary<string, string>>("complex");
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
                { "test1", "shawn" },
                { "test2", "wedge" },
                { "test3", 42 },
            };

            var newActivity = new Activity("1", "test", "1");
            newActivity.SetData("complex", dict);
            newActivity.SetData("second", second);
            newActivity.SetData("third", third);
            newActivity.SetData("customc", new CustomClass
            {
                TestString = "string",
                TestInt = 123,
                TestDouble = 42.2,
            });

            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
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

            var response = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(response);

            var activities = (await this.UserFeed.GetActivitiesAsync(0, 1)).Results;
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
                To = new List<string> { "flat:remotefeed1" },
            };
            var addedActivity = await this.UserFeed.AddActivityAsync(newActivity);
            Assert.IsNotNull(addedActivity);
            Assert.IsNotNull(addedActivity.To);
            Assert.AreEqual(1, addedActivity.To.CountOrFallback());
            Assert.AreEqual("flat:remotefeed1", addedActivity.To.First());

            var activities = (await Client.Feed("flat", "remotefeed1").GetActivitiesAsync(0, 1)).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var first = activities.First();
            Assert.AreEqual(first.Actor, "multi1");
        }
    }
}