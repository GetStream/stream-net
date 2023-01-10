using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Stream;
using Stream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class EnrichedActivityTests : TestBase
    {
        [Test]
        public async Task TestEnrich()
        {
            var a = new Activity("johhny", "add", "post");
            var act = await this.UserFeed.AddActivityAsync(a);
            var reaction = await Client.Reactions.AddAsync("like", act.Id, "johhny");

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent().Own().Counts()));

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
        public async Task TestGetEnrichedFlatActivitiesByID()
        {
            var userId = System.Guid.NewGuid().ToString();
            const string userName = "user name";
            var user = await Client.Users.AddAsync(userId, new Dictionary<string, object> { ["name"] = userName });
            var newActivity1 = new Activity(user.Ref(), "test", "1");
            var newActivity2 = new Activity(user.Ref(), "test", "2");
            var newActivity3 = new Activity(user.Ref(), "other", "2");
            var addedActivities = new List<Activity>();

            var response = await this.UserFeed.AddActivityAsync(newActivity1);
            addedActivities.Add(response);
            response = await this.UserFeed2.AddActivityAsync(newActivity2);
            addedActivities.Add(response);
            response = await this.FlatFeed.AddActivityAsync(newActivity3);
            addedActivities.Add(response);

            var activities = (await Client.Batch.GetEnrichedActivitiesAsync(addedActivities.Select(a => a.Id))).Results;
            Assert.IsNotNull(activities);
            Assert.AreEqual(addedActivities.Count, activities.Count());

            activities.ForEach(a =>
            {
                var found = addedActivities.Find(x => x.Id == a.Id);
                Assert.NotNull(found);

                Assert.AreEqual(userId, a.Actor.Id);
                var userData = a.Actor.GetData<Dictionary<string, object>>("data");
                Assert.AreEqual(userName, userData["name"]);
            });
        }

        [Test]
        public async Task TestGetEnrichedFlatActivitiesByIDWithReactions()
        {
            var userId = System.Guid.NewGuid().ToString();
            var user = await Client.Users.AddAsync(userId);
            var newActivity = new Activity(user.Ref(), "test", "1");
            newActivity = await this.UserFeed.AddActivityAsync(newActivity);

            await Client.Reactions.AddAsync("upvote", newActivity.Id, user.Id, new Dictionary<string, object> { ["reactionProp"] = "reactionPropValue" });

            var activities = (await Client.Batch.GetEnrichedActivitiesAsync(
                new[] { newActivity.Id },
                new GetOptions().WithReaction(ReactionOption.With().Counts().Recent()))).Results;

            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count());

            var enrichedActivity = activities.Single();
            Assert.AreEqual(userId, enrichedActivity.Actor.Id);

            Assert.IsNotNull(enrichedActivity.ReactionCounts);
            Assert.AreEqual(1, enrichedActivity.ReactionCounts.Count);
            Assert.AreEqual("upvote", enrichedActivity.ReactionCounts.Keys.Single());
            Assert.AreEqual(1, enrichedActivity.ReactionCounts["upvote"]);

            Assert.IsNotNull(enrichedActivity.LatestReactions);
            Assert.AreEqual(1, enrichedActivity.LatestReactions.Count);
            Assert.AreEqual("upvote", enrichedActivity.LatestReactions.Keys.Single());

            var enrichedReaction = enrichedActivity.LatestReactions["upvote"];
            Assert.IsNotNull(enrichedReaction.First().Data);
            Assert.AreEqual("reactionPropValue", enrichedReaction.First().Data["reactionProp"]);
        }

        [Test]
        public async Task TestEnrich_Collection()
        {
            var c = new CollectionObject(Guid.NewGuid().ToString());
            c.SetData("field", "testing_value");
            await Client.Collections.UpsertAsync("items", c);
            var cRef = Client.Collections.Ref("items", c);

            var a = new Activity("actor-1", "add", cRef);
            await this.UserFeed.AddActivityAsync(a);

            var plain = await this.UserFeed.GetFlatActivitiesAsync();
            Assert.AreEqual(cRef, plain.Results.First().Object);

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync();
            Assert.AreEqual(1, enriched.Results.Count());

            var act = enriched.Results.First();
            Assert.NotNull(act.Actor);
            Assert.AreEqual("actor-1", act.Actor.Id);
            Assert.AreEqual(c.Id, act.Object.Id);
            var dataJobject = act.Object.GetData<Dictionary<string, object>>("data");
            Assert.AreEqual("testing_value", dataJobject["field"].ToString());
        }

        [Test]
        public async Task TestEnrich_User()
        {
            var userData = new Dictionary<string, object>()
            {
                { "is_admin", true },
                { "nickname", "bobby" },
            };
            var u = await Client.Users.AddAsync(Guid.NewGuid().ToString(), userData);
            var uRef = u.Ref();

            var a = new Activity(uRef, "add", "post");
            await this.UserFeed.AddActivityAsync(a);

            var plain = await this.UserFeed.GetFlatActivitiesAsync();
            Assert.AreEqual(plain.Results.First().Actor, uRef);

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync();

            Assert.AreEqual(1, enriched.Results.Count());

            var act = enriched.Results.First();
            Assert.AreEqual(u.Id, act.Actor.Id);
            Assert.AreEqual(userData, act.Actor.GetData<Dictionary<string, object>>("data"));
        }

        [Test]
        public async Task TestEnrich_OwnReaction()
        {
            var a = new Activity("johhny", "add", "post");
            var act = await this.UserFeed.AddActivityAsync(a);
            var reaction = await Client.Reactions.AddAsync("like", act.Id, "johhny");

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Own()));

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
                { "is_admin", true },
                { "nickname", "bobby" },
            };
            var u = await Client.Users.AddAsync(Guid.NewGuid().ToString(), userData);
            var uRef = u.Ref();

            var a = new Activity(uRef, "add", "post");
            var act = await this.UserFeed.AddActivityAsync(a);

            var reaction = await Client.Reactions.AddAsync("like", act.Id, u.Id);

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Own()));

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
            var act = await this.UserFeed.AddActivityAsync(a);
            var reaction = await Client.Reactions.AddAsync("like", act.Id, "johhny");

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.AreEqual(reaction.Id, enrichedAct.LatestReactions[reaction.Kind].First().Id);
            Assert.AreEqual(reaction.Kind, enrichedAct.LatestReactions[reaction.Kind].First().Kind);
            Assert.AreEqual(reaction.UserId, enrichedAct.LatestReactions[reaction.Kind].First().UserId);

            var comment = await Client.Reactions.AddAsync("comment", act.Id, "bobby");
            await Client.Reactions.AddAsync("comment", act.Id, "tony");
            await Client.Reactions.AddAsync("comment", act.Id, "rupert");

            enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent().KindFilter("comment").KindFilter("upvote")));

            Assert.AreEqual(1, enriched.Results.Count());

            enrichedAct = enriched.Results.First();

            Assert.False(enrichedAct.LatestReactions.ContainsKey(reaction.Kind));
            Assert.True(enrichedAct.LatestReactions.ContainsKey(comment.Kind));

            enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Recent().RecentLimit(1)));

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
            var act = await this.UserFeed.AddActivityAsync(a);
            var reactionLike = await Client.Reactions.AddAsync("like", act.Id, "johhny");
            var reactionComment = await Client.Reactions.AddAsync("comment", act.Id, "johhny");
            var reactionLike2 = await Client.Reactions.AddAsync("like", act.Id, "timmeh");

            var enriched = await this.UserFeed.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Counts()));

            Assert.AreEqual(1, enriched.Results.Count());

            var enrichedAct = enriched.Results.First();

            Assert.True(enrichedAct.ReactionCounts.ContainsKey(reactionLike.Kind));
            Assert.True(enrichedAct.ReactionCounts.ContainsKey(reactionComment.Kind));
            Assert.AreEqual(2, enrichedAct.ReactionCounts[reactionLike.Kind]);
            Assert.AreEqual(1, enrichedAct.ReactionCounts[reactionComment.Kind]);
        }
    }
}