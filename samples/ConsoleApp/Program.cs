using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Stream;
using Stream.Models;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create a client, find your API keys here https://getstream.io/dashboard/
            var client = new StreamClient(Environment.GetEnvironmentVariable("STREAM_API_KEY"), Environment.GetEnvironmentVariable("STREAM_API_SECRET"));
            // From this IStreamClient instance you can access a variety of API endpoints,
            // and it is also a factory class for other classes, such as IBatch, ICollections
            // IReactions, IStreamFeed etc. Let's take a look at IStreamFeed now.

            // Reference a feed
            var userFeed1 = client.Feed("user", "1");

            // Get 10 activities starting from the 5th (slow offset pagination)
            var results = await userFeed1.GetActivitiesAsync(5, 10);

            // Create a new activity
            var activity = new Activity("1", "like", "3")
            {
                ForeignId = "post:42"
            };

            await userFeed1.AddActivityAsync(activity);

            // Create a complex activity
            activity = new Activity("1", "run", "1")
            {
                ForeignId = "run:1"
            };
            var course = new Dictionary<string, object>();
            course["name"] = "Shevlin Park";
            course["distance"] = 10;
            activity.SetData("course", course);
            await userFeed1.AddActivityAsync(activity);

            // Remove an activity by its id
            await userFeed1.RemoveActivityAsync("e561de8f-00f1-11e4-b400-0cc47a024be0");

            // Remove activities by their foreign_id
            await userFeed1.RemoveActivityAsync("post:42", true);

            // Let user 1 start following user 42's flat feed
            await userFeed1.FollowFeedAsync("flat", "42");

            // Let user 1 stop following user 42's flat feed
            await userFeed1.UnfollowFeedAsync("flat", "42");

            // Retrieve first 10 followers of a feed
            await userFeed1.FollowersAsync(0, 10);

            // Retrieve 2 to 10 followers
            await userFeed1.FollowersAsync(2, 10);

            // Retrieve 10 feeds followed by $user_feed_1
            await userFeed1.FollowingAsync(0, 10);

            // Retrieve 10 feeds followed by $user_feed_1 starting from the 10th (2nd page)
            await userFeed1.FollowingAsync(10, 20);

            // Check if $user_feed_1 follows specific feeds
            await userFeed1.FollowingAsync(0, 2, new[] { "user:42", "user:43" });

            // Follow stats
            // Count the number of users following this feed
            // Count the number of tags are followed by this feed
            var stats = await userFeed1.FollowStatsAsync(new[] { "user" }, new[] { "tags" });
            Console.WriteLine(stats.Results.Followers.Count);
            Console.WriteLine(stats.Results.Following.Count);

            // Retrieve activities by their ids
            var ids = new[] { "e561de8f-00f1-11e4-b400-0cc47a024be0", "a34ndjsh-00f1-11e4-b400-0c9jdnbn0eb0" };
            var activities = await client.Batch.GetActivitiesByIdAsync(ids);

            // Retrieve activities by their ForeignID/Time
            var foreignIDTimes = new[] { new ForeignIdTime("fid-1", DateTime.Parse("2000-08-19T16:32:32")), new ForeignIdTime("fid-2", DateTime.Parse("2000-08-21T16:32:32")) };
            activities = await client.Batch.GetActivitiesByForeignIdAsync(foreignIDTimes);

            // Partially update an activity
            var set = new Dictionary<string, object>();
            set.Add("custom_field", "new value");
            var unset = new[] { "field to remove" };

            // By id
            await client.ActivityPartialUpdateAsync("e561de8f-00f1-11e4-b400-0cc47a024be0", null, set, unset);

            // By foreign id and time
            var fidTime = new ForeignIdTime("fid-1", DateTime.Parse("2000-08-19T16:32:32"));
            await client.ActivityPartialUpdateAsync(null, fidTime, set, unset);

            // Add a reaction to an activity
            var activityData = new Activity("bob", "cook", "burger")
            {
                ForeignId = "post:42"
            };
            activity = await userFeed1.AddActivityAsync(activity);
            var r = await client.Reactions.AddAsync("comment", activity.Id, "john");

            // Add a reaction to a reaction
            var child = await client.Reactions.AddChildAsync(r, "upvote", "john");

            // Enrich feed results
            var userData = new Dictionary<string, object>()
            {
                {"is_admin", true},
                {"nickname","bobby"}
            };
            var u = await client.Users.AddAsync("timmy", userData);
            var userRef = u.Ref();
            var a = new Activity(userRef, "add", "post");
            var plainActivity = await userFeed1.AddActivityAsync(a);
            // Here plainActivity.Actor is just a plain string containing the user ref
            var enriched = await userFeed1.GetEnrichedFlatActivitiesAsync();
            var actor = enriched.Results.First();
            var userID = actor.GetData<string>("id");
            var data = actor.GetData<Dictionary<string, object>>("data"); //this is `userData`

            // Enrich feed results with reactions
            activityData = new Activity("bob", "cook", "burger")
            {
                ForeignId = "post:42"
            };
            activity = await userFeed1.AddActivityAsync(activity);
            var com = await client.Reactions.AddAsync("comment", activity.Id, "john");
            var like = await client.Reactions.AddAsync("like", activity.Id, "maria");

            // Fetch reaction counts grouped by reaction kind
            enriched = await userFeed1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Counts()));
            var enrichedActivity = enriched.Results.First();
            Console.WriteLine(enrichedActivity.ReactionCounts["like"]); // 1
            Console.WriteLine(enrichedActivity.ReactionCounts["comment"]); // 1

            // Fetch reactions grouped by reaction kind
            enriched = await userFeed1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Own()));
            enrichedActivity = enriched.Results.First();
            Console.WriteLine(enrichedActivity.OwnReactions["like"]); // is the $like reaction
            Console.WriteLine(enrichedActivity.OwnReactions["comment"]); // is the $comment reaction

            // All reactions enrichment can be selected
            enriched = await userFeed1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Counts().Own().Recent()));

            // Personalization
            var input = new Dictionary<string, object>
            {
                {"feed_slug", "my_personalized_feed"},
                {"user_id", "john"},
                {"limit", 20},
                {"ranking", "my_ranking"}
            };
            var response = await client.Personalization.GetAsync("my_endpoint", input);

            // File & Image Upload
            using (var fs = File.OpenRead("../../tests/helloworld.txt"))
            {
                var fileUpload = await client.Files.UploadAsync(fs, "helloworld");
            }
            using (var fs = File.OpenRead("../../tests/helloworld.jpg"))
            {
                var imageupload = await client.Images.UploadAsync(fs, "helloworld", "image/jpeg");
            }

            // Use fileUpload.File and imageUpload.File afterwards
            // Open graph
            var og = await client.OgAsync("https://google.com");
        }
    }

}

