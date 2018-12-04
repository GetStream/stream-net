stream-net
===========

[![Build status](https://ci.appveyor.com/api/projects/status/nhnrdhf64clbcv19/branch/master?svg=true)](https://ci.appveyor.com/project/tbarbugli/stream-net/branch/master)

[stream-net](https://github.com/GetStream/stream-net) is a .Net client for [Stream](https://getstream.io/).

You can sign up for a Stream account at https://getstream.io/get_started.

### Installation via Nuget

```sh
nuget install stream-net
```

### Usage

```c#
// Create a client, find your API keys here https://getstream.io/dashboard/
var client = new StreamClient("YOUR_API_KEY","API_KEY_SECRET");

// Reference a feed
var userFeed1 = client.Feed("user", "1");

// Get 20 activities starting from activity with id last_id (fast id offset pagination)
var results = await userFeed1.GetActivities(0, 20, FeedFilter.Where().IdLessThan(last_id));

// Get 10 activities starting from the 5th (slow offset pagination)
var results = await userFeed1.GetActivities(5, 10);

// Create a new activity
var activity = new Activity("1", "like", "3")
{
	ForeignId = "post:42"
};
userFeed1.AddActivity(activity);

// Create a complex activity
var activity = new Activity("1", "run", "1")
{
	ForeignId = "run:1"
};
var course = new Dictionary<string,object>();
course["name"] = "Shevlin Park";
course["distance"] = 10;
activity.SetData("course", course);
userFeed1.AddActivity(activity);

// Remove an activity by its id
userFeed1.RemoveActivity("e561de8f-00f1-11e4-b400-0cc47a024be0");

// Remove activities by their foreign_id
userFeed1.RemoveActivity("post:42", true);

// Let user 1 start following user 42's flat feed
userFeed1.FollowFeed("flat", "42");

// Let user 1 stop following user 42's flat feed
userFeed1.UnfollowFeed("flat", "42");

// Delete a feed (and its content)
userFeed1.Delete();

// Retrieve first 10 followers of a feed
userFeed1.Followers(0, 10);

// Retrieve 2 to 10 followers
userFeed1.Followers(2, 10);

// Retrieve 10 feeds followed by $user_feed_1
userFeed1.Following(0, 10);

// Retrieve 10 feeds followed by $user_feed_1 starting from the 10th (2nd page)
userFeed1.Following(10, 20);

// Check if $user_feed_1 follows specific feeds
userFeed1.Following(0, 2, new String[] { "user:42", "user:43" });

// Retrieve activities by their ids
var ids = new string[] { "e561de8f-00f1-11e4-b400-0cc47a024be0", "a34ndjsh-00f1-11e4-b400-0c9jdnbn0eb0" };
var activities = await client.Batch.GetActivities(ids)

// Retrieve activities by their ForeignID/Time
var foreignIDTimes = new ForeignIDTime[] {new ForeignIDTime("fid-1", DateTime.Parse("2000-08-19T16:32:32")), new Stream.ForeignIDTime("fid-2",  DateTime.Parse("2000-08-21T16:32:32"))};
var activities = await client.Batch.GetActivities(null, foreignIDTimes)

//Partially update an activity
var set = new GenericData();
set.SetData("custom_field", "new value");
var unset = new string[]{"field to remove"};

//by id
await client.ActivityPartialUpdate("e561de8f-00f1-11e4-b400-0cc47a024be0", null, set, unset);

//by foreign id and time
var fidTime = new ForeignIDTime("fid-1", DateTime.Parse("2000-08-19T16:32:32"));
await client.ActivityPartialUpdate(null, fidTime, set, unset);

//add a reaction to an activity
var activityData = new Activity("bob", "cook", "burger")
{
	ForeignId = "post:42"
};
var activity = await userFeed1.AddActivity(activity);


var r = await client.Reactions.Add("comment", activity.Id, "john");

//add a reaction to a reaction
var child = await client.Reactions.AddChild(r.ID, "upvote", activity.Id, "john");

//enrich feed results
var userData = new Dictionary<string, object>()
{
	{"is_admin", true},
	{"nickname","bobby"}
};
var u = await client.Users.Add("timmy", userData);
var userRef = u.Ref();

var a = new Stream.Activity(uRef, "add", "post");
var plainActivity = await userFeed1.AddActivity(a);

//plainActivity.Actor is just a plain string containing the user ref

var enriched = await this._user1.GetEnrichedFlatActivities();
var actor = enriched.Results.First();
if (actor.IsEnriched)
{
	//The `Enriched` propery contains the user object instead of just the ref
	var userID = actor.Enriched.GetData<string>("id");
	var data = actor.Enriched.GetData<Dictionary<string,object>>("data"); //this is `userData`
}


//enrich feed results with reactions

var activityData = new Activity("bob", "cook", "burger")
{
	ForeignId = "post:42"
};
var activity = await userFeed1.AddActivity(activity);


var com = await client.Reactions.Add("comment", activity.Id, "john");
var like = await client.Reactions.Add("like", activity.Id, "maria");

// fetch reaction counts grouped by reaction kind
var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Counts())));

var enrichedActivity = enriched.Results.First();
Console.WriteLine(enrichedActivity.ReactionCounts["like"]); //1
Console.WriteLine(enrichedActivity.ReactionCounts["comment"]); //1

// fetch reactions grouped by reaction kind
var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Own())));

var enrichedActivity = enriched.Results.First();
Console.WriteLine(enrichedActivity.OwnReactions["like"]); // is the $like reaction
Console.WriteLine(enrichedActivity.OwnReactions["comment"]); // is the $comment reaction

// all reactions enrichment can be selected
var enriched = await this._user1.GetEnrichedFlatActivities(GetOptions.Default.WithReaction(ReactionOption.With().Counts().Own().Recent()));
```

### Copyright and License Information

Copyright (c) 2015-2018 Shawn Beach, Stream.io Inc, and individual contributors. All rights reserved.

See the file "LICENSE" for information on the history of this software, terms & conditions for usage, and a DISCLAIMER OF ALL WARRANTIES.
