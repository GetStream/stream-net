using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamFeed
    {
        static Regex _feedRegex = new Regex(@"^\w+$", RegexOptions.Compiled);
        static Regex _userRegex = new Regex(@"^[-\w]+$", RegexOptions.Compiled);

        readonly StreamClient _client;
        readonly string _feedSlug;
        readonly string _userId;

        internal StreamFeed(StreamClient client, string feedSlug, string userId, string token)
        {
            if (!_feedRegex.IsMatch(feedSlug))
                throw new ArgumentException("Feed slug can only contain alphanumeric characters or underscores");
            if (!_userRegex.IsMatch(userId))
                throw new ArgumentException("User id can only contain alphanumeric characters, underscores or dashes");

            Token = token;
            _client = client;
            _feedSlug = feedSlug;
            _userId = userId;
            FeedTokenId = string.Format("{0}{1}", _feedSlug, _userId);
            UrlPath = string.Format("feed/{0}/{1}", _feedSlug, _userId);
        }

        internal string FeedTokenId { get; private set; }

        internal string FeedId
        {
            get
            {
                return string.Format("{0}:{1}", _feedSlug, _userId);
            }
        }

        public string Token { get; private set; }

        public string ReadOnlyToken
        {
            get
            {
                return _client.JWToken(FeedTokenId);
            }
        }

        public string UrlPath { get; private set; }

        /// <summary>
        /// Add an activity to the feed
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>An activity with ID and Date supplied</returns>
        public async Task<Activity> AddActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity", "Must have an activity to add");

            var request = _client.BuildFeedRequest(this, "/", Method.POST);
            request.AddParameter("application/json", activity.ToJson(this._client), ParameterType.RequestBody);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return Activity.FromJson(response.Content);

            throw StreamException.FromResponse(response);
        }

        internal string ToActivitiesJson(IEnumerable<Activity> activities)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("activities");
                writer.WriteStartArray();

                activities.ForEach((a) =>
                {
                    writer.WriteRawValue(a.ToJson(this._client));
                });

                writer.WriteEnd();
                writer.WriteEndObject();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Add a list of activities
        /// </summary>
        /// <param name="activities"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Activity>> AddActivities(IEnumerable<Activity> activities)
        {
            if (activities.SafeCount() == 0)
                throw new ArgumentNullException("activities", "Must have activities to add");

            var request = _client.BuildFeedRequest(this, "/", Method.POST);
            request.AddParameter("application/json", ToActivitiesJson(activities), ParameterType.RequestBody);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        /// <summary>
        /// Update an activity to the feed
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public Task UpdateActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity", "Must have an activity to add");
            return UpdateActivities(new Activity[] { activity });
        }

        /// <summary>
        /// Update a list of activities, Maximum length is 100
        /// </summary>
        /// <param name="activities"></param>
        /// <returns></returns>
        public async Task UpdateActivities(IEnumerable<Activity> activities)
        {
            if (activities.SafeCount() == 0)
                throw new ArgumentNullException("activities", "Must have activities to add");
            if (activities.SafeCount() > 100)
                throw new ArgumentNullException("activities", "Maximum length is 100");

            var request = _client.BuildActivitiesRequest(this);
            request.AddParameter("application/json", ToActivitiesJson(activities), ParameterType.RequestBody);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        /// <summary>
        /// Remove an activity
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="foreignId"></param>
        /// <returns></returns>
        public async Task RemoveActivity(string activityId, bool foreignId = false)
        {
            var request = _client.BuildFeedRequest(this, "/" + activityId + "/", Method.DELETE);
            if (foreignId)
                request.AddQueryParameter("foreign_id", "1");
            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        internal IEnumerable<Activity> GetResults(string json)
        {
            JObject obj = JObject.Parse(json);
            foreach (var prop in obj.Properties())
            {
                if ((prop.Name == "results") || (prop.Name == "activities"))
                {
                    // get the array
                    var array = prop.Value as JArray;
                    foreach (var val in array)
                        yield return Activity.FromJson((JObject)val);
                }
            }
        }

        public async Task<IEnumerable<Activity>> GetActivities(int offset = 0, int limit = 20, FeedFilter filter = null, ActivityMarker marker = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset must be greater than or equal to zero");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit", "Limit must be greater than or equal to zero");

            var request = _client.BuildFeedRequest(this, "/", Method.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            // filter if needed
            if (filter != null)
                filter.Apply(request);

            // marker if needed
            if (marker != null)
                marker.Apply(request);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        internal async Task<StreamResponse<T>> GetWithOptions<T>(GetOptions options = null) where T : Activity
        {
            // build request
            options = options ?? GetOptions.Default;
            var request = _client.BuildFeedRequest(this, "/", Method.GET);
            options.Apply(request);

            // make request
            var response = await _client.MakeRequest(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            // handle response
            var result = new StreamResponse<T>();
            JObject obj = JObject.Parse(response.Content);
            foreach (var prop in obj.Properties())
            {
                switch (prop.Name)
                {
                    case "results":
                    case "activities":
                        {
                            // get the results
                            var array = prop.Value as JArray;
                            result.Results = array.Select(a => Activity.FromJson((JObject)a) as T).ToList();
                            break;
                        }
                    case "unseen":
                        {
                            result.Unseen = prop.Value.Value<long>();
                            break;
                        }
                    case "unread":
                        {
                            result.Unread = prop.Value.Value<long>();
                            break;
                        }
                    case "duration":
                        {
                            result.Duration = prop.Value.Value<String>();
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        public Task<StreamResponse<Activity>> GetFlatActivities(GetOptions options = null)
        {
            return GetWithOptions<Activity>(options);
        }

        public Task<StreamResponse<AggregateActivity>> GetAggregateActivities(GetOptions options = null)
        {
            return GetWithOptions<AggregateActivity>(options);
        }

        public Task<StreamResponse<NotificationActivity>> GetNotificationActivities(GetOptions options = null)
        {
            return GetWithOptions<NotificationActivity>(options);
        }

        public async Task FollowFeed(StreamFeed feedToFollow)
        {
            if (feedToFollow == null)
                throw new ArgumentNullException("feedToFollow", "Must have a feed to follow");
            if (feedToFollow.FeedTokenId == this.FeedTokenId)
                throw new ArgumentException("Cannot follow myself");

            var request = _client.BuildFeedRequest(this, "/follows/", Method.POST);
            request.AddJsonBody(new
            {
                target = feedToFollow.FeedId,
                target_token = feedToFollow.Token
            });

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public Task FollowFeed(string targetFeedSlug, string targetUserId)
        {
            return FollowFeed(this._client.Feed(targetFeedSlug, targetUserId));
        }

        public async Task UnfollowFeed(StreamFeed feedToUnfollow, bool keepHistory = false)
        {
            if (feedToUnfollow == null)
                throw new ArgumentNullException("feedToUnfollow", "Must have a feed to unfollow");
            if (feedToUnfollow.FeedTokenId == this.FeedTokenId)
                throw new ArgumentException("Cannot unfollow myself");

            var request = _client.BuildFeedRequest(this, "/follows/" + feedToUnfollow.FeedId + "/", Method.DELETE);
            request.AddQueryParameter("keep_history", keepHistory.ToString());

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public Task UnfollowFeed(String targetFeedSlug, String targetUserId, bool keepHistory = false)
        {
            return UnfollowFeed(this._client.Feed(targetFeedSlug, targetUserId), keepHistory);
        }

        internal class FollowersResponse
        {
            public IEnumerable<Follower> results { get; set; }
        }

        public async Task<IEnumerable<Follower>> Followers(int offset = 0, int limit = 25, string[] filterBy = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset must be greater than or equal to zero");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit", "Limit must be greater than or equal to zero");

            var request = _client.BuildFeedRequest(this, "/followers/", Method.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", String.Join(",", filterBy));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return JsonConvert.DeserializeObject<FollowersResponse>(response.Content).results;
        }

        public async Task<IEnumerable<Follower>> Following(int offset = 0, int limit = 25, String[] filterBy = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset must be greater than or equal to zero");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit", "Limit must be greater than or equal to zero");

            var request = _client.BuildFeedRequest(this, "/following/", Method.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", String.Join(",", filterBy));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return JsonConvert.DeserializeObject<FollowersResponse>(response.Content).results;
        }

        /// <summary>
        /// Delete the feed
        /// </summary>
        public async Task Delete()
        {
            var request = _client.BuildFeedRequest(this, "/", Method.DELETE);
            var response = await _client.MakeRequest(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }
    }
}
