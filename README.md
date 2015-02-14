# stream-net

### Installation via Nuget

Coming soon!

### Usage

```c#
// Create a client, find your API keys here https://getstream.io/dashboard/
var client = new StreamClient('YOUR_API_KEY', 'API_KEY_SECRET');

// Reference a feed
var userFeed1 = client.Feed('user', '1');

// Create a new activity
var activity = new Activity("1", "like", "3") 
{
	ForeignId = "post:42"
};  
userFeed1.AddActivity(activity);

```