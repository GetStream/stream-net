# Official .NET SDK for [Stream](https://getstream.io/activity-feeds/)


[![Build status](https://github.com/GetStream/stream-net/actions/workflows/ci.yaml/badge.svg)](https://github.com/GetStream/stream-net/actions/workflows/ci.yaml) [![NuGet Badge](https://buildstats.info/nuget/stream-net)](https://www.nuget.org/packages/stream-net/)

<p align="center">
    <img src="./assets/logo.svg" width="50%" height="50%">
</p>
<p align="center">
    Official .NET API client for Stream, a service for building scalable newsfeeds and activity streams.
    <br />
    <a href="https://getstream.io/activity-feeds/docs/"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/GetStream/stream-net/tree/master/samples">Code Samples</a>
    ·
    <a href="https://github.com/GetStream/stream-net/issues">Report Bug</a>
    ·
    <a href="https://github.com/GetStream/stream-net/issues">Request Feature</a>
</p>

> ## 🚨 Breaking changes in v5.0 <
> In v1.0.0, [we have refactored the library](https://github.com/GetStream/stream-net/pull/67) to be more maintainable in the future.
> Most importantly, we got rid of some complex internal logic (such as tricky json serialization and deserialization, code organization improvements etc.).
> Also, we made the library more modern such as adding `Async` postfix to async methods. All public
> methods have documentation now and a link to the official docs now. This README file's code snippets are updated to reflect the new changes.
>
> ### Breaking changes:
> - All async methods have `Async` postfix.
> - Model classes have been moved into `Stream.Models` namespace.
> - All client classes have interfaces now, and `Ref()` methods are not static anymore. This will make it easier to the consumers of this library to unit test them.


## 📝 About Stream

You can sign up for a Stream account at our [Get Started](https://getstream.io/get_started/) page.

You can use this library to access chat API endpoints server-side.

For the client-side integrations (web and mobile) have a look at the JavaScript, iOS and Android SDK libraries ([docs](https://getstream.io/activity-feeds/docs/)).

## ⚙️ Installation

```shell
$ nuget install stream-net
```

## ✨ Getting started

```c#
// Create a client, find your API keys here https://getstream.io/dashboard/
var client = new StreamClient("YOUR_API_KEY", "API_KEY_SECRET");
// From this IStreamClient instance you can access a variety of API endpoints,
// and it is also a factory class for other classes, such as IBatch, ICollections
// IReactions, IStreamFeed etc. Note: all of these clients can be used as a
// singleton as they do not store state.
// Let's take a look at IStreamFeed now.

// Reference a feed
var userFeed1 = client.Feed("user", "1");

// Get 20 activities starting from activity with id last_id (fast id offset pagination)
var results = await userFeed1.GetActivitiesAsync(0, 20, FeedFilter.Where().IdLessThan(last_id));

// Get 10 activities starting from the 5th (slow offset pagination)
var results = await userFeed1.GetActivitiesAsync(5, 10);

// Create a new activity
var activity = new Activity("1", "like", "3")
{
    ForeignId = "post:42"
};

await userFeed1.AddActivityAsync(activity);

// Create a complex activity
var activity = new Activity("1", "run", "1")
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
var activities = await client.Batch.GetActivitiesByForeignIdAsync(foreignIDTimes);

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
var activity = await userFeed1.AddActivityAsync(activity);
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
var activityData = new Activity("bob", "cook", "burger")
{
    ForeignId = "post:42"
};
var activity = await userFeed1.AddActivityAsync(activity);
var com = await client.Reactions.AddAsync("comment", activity.Id, "john");
var like = await client.Reactions.AddAsync("like", activity.Id, "maria");

// Fetch reaction counts grouped by reaction kind
var enriched = await userFeed1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Counts()));
var enrichedActivity = enriched.Results.First();
Console.WriteLine(enrichedActivity.ReactionCounts["like"]); // 1
Console.WriteLine(enrichedActivity.ReactionCounts["comment"]); // 1

// Fetch reactions grouped by reaction kind
var enriched = await userFeed1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Own()));
var enrichedActivity = enriched.Results.First();
Console.WriteLine(enrichedActivity.OwnReactions["like"]); // is the $like reaction
Console.WriteLine(enrichedActivity.OwnReactions["comment"]); // is the $comment reaction

// All reactions enrichment can be selected
var enriched = await userFeed1.GetEnrichedFlatActivitiesAsync(GetOptions.Default.WithReaction(ReactionOption.With().Counts().Own().Recent()));

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
var fileUpload = await client.Files.UploadAsync(stream, name, contentType);
var imageupload = await client.Images.UploadAsync(stream, name, contentType);

// Use fileUpload.File and imageUpload.File afterwards
// Open graph
var og = await client.OgAsync("https://google.com");
```

## ✍️ Contributing

We welcome code changes that improve this library or fix a problem, please make sure to follow all best practices and add tests if applicable before submitting a Pull Request on Github. We are very happy to merge your code in the official repository. Make sure to sign our [Contributor License Agreement (CLA)](https://docs.google.com/forms/d/e/1FAIpQLScFKsKkAJI7mhCr7K9rEIOpqIDThrWxuvxnwUq2XkHyG154vQ/viewform) first. See our [license file](./LICENSE) for more details.

Head over to [CONTRIBUTING.md](./CONTRIBUTING.md) for some development tips.

## 🧑‍💻 We are hiring!

We've recently closed a [$38 million Series B funding round](https://techcrunch.com/2021/03/04/stream-raises-38m-as-its-chat-and-activity-feed-apis-power-communications-for-1b-users/) and we keep actively growing.
Our APIs are used by more than a billion end-users, and you'll have a chance to make a huge impact on the product within a team of the strongest engineers all over the world.

Check out our current openings and apply via [Stream's website](https://getstream.io/team/#jobs).
