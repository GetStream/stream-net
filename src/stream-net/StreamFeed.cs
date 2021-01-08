﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamFeed : IStreamFeed
    {
        static Regex _feedRegex = new Regex(@"^\w+$", RegexOptions.Compiled);
        static Regex _userRegex = new Regex(@"^[-\w]+$", RegexOptions.Compiled);

        readonly StreamClient _client;
        readonly string _feedSlug;
        readonly string _userId;

        internal StreamFeed(StreamClient client, string feedSlug, string userId)
        {
            if (!_feedRegex.IsMatch(feedSlug))
                throw new ArgumentException("Feed slug can only contain alphanumeric characters or underscores");
            if (!_userRegex.IsMatch(userId))
                throw new ArgumentException("User id can only contain alphanumeric characters, underscores or dashes");

            _client = client;
            _feedSlug = feedSlug;
            _userId = userId;
            UrlPath = string.Format("feed/{0}/{1}", _feedSlug, _userId);
            EnrichedPath = "enrich/" + UrlPath;
        }

        public string FeedId
        {
            get
            {
                return string.Format("{0}:{1}", _feedSlug, _userId);
            }
        }

        public string UrlPath { get; private set; }
        public string EnrichedPath { get; private set; }

        /// <summary>
        /// Add an activity to the feed
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>An activity with ID and Date supplied</returns>
        public async Task<Activity> AddActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity", "Must have an activity to add");

            var request = _client.BuildFeedRequest(this, "/", HttpMethod.POST);
            request.SetJsonBody(activity.ToJson(this._client));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return Activity.FromJson(response.Content);

            throw StreamException.FromResponse(response);
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

            var request = _client.BuildFeedRequest(this, "/", HttpMethod.POST);
            request.SetJsonBody(Activity.ToActivitiesJson(activities, this._client));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return Activity.GetResults(response.Content);

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
            request.SetJsonBody(Activity.ToActivitiesJson(activities, this._client));

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
            var request = _client.BuildFeedRequest(this, "/" + activityId + "/", HttpMethod.DELETE);
            if (foreignId)
                request.AddQueryParameter("foreign_id", "1");
            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public async Task<IEnumerable<Activity>> GetActivities(int offset = 0, int limit = 20, FeedFilter filter = null, ActivityMarker marker = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset must be greater than or equal to zero");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit", "Limit must be greater than or equal to zero");

            var request = _client.BuildFeedRequest(this, "/", HttpMethod.GET);
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
                return Activity.GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<UpdateToTargetsResponse> UpdateActivityToTargets(ForeignIDTime foreignIDTime,
            IEnumerable<string> adds = null,
            IEnumerable<string> newTargets = null,
            IEnumerable<string> removed = null)
        {
            var payload = new Dictionary<string, object>()
            {
                {"foreign_id" , foreignIDTime.ForeignID},
                {"time" , foreignIDTime.Time.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}
            };
            if (adds != null)
                payload["added_targets"] = adds.ToList();
            if (newTargets != null)
                payload["new_targets"] = newTargets.ToList();
            if (removed != null)
                payload["removed_targets"] = removed.ToList();

            var endpoint = string.Format("feed_targets/{0}/{1}/activity_to_targets/", this._feedSlug, this._userId);
            var request = this._client.BuildAppRequest(endpoint, HttpMethod.POST);
            request.SetJsonBody(JsonConvert.SerializeObject(payload));
            var response = await this._client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return UpdateToTargetsResponse.FromJson(response.Content);

            throw StreamException.FromResponse(response);
        }

        internal async Task<StreamResponse<T>> GetWithOptions<T>(GetOptions options = null) where T : Activity
        {
            // build request
            options = options ?? GetOptions.Default;
            var request = _client.BuildFeedRequest(this, "/", HttpMethod.GET);
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

        internal async Task<StreamResponse<T>> GetEnriched<T>(GetOptions options = null) where T : EnrichedActivity
        {
            // build request
            options = options ?? GetOptions.Default;
            var request = _client.BuildEnrichedFeedRequest(this, "/", HttpMethod.GET);
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
                            result.Results = array.Select(a => EnrichedActivity.FromJson((JObject)a) as T).ToList();
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

        public Task<StreamResponse<EnrichedActivity>> GetEnrichedFlatActivities(GetOptions options = null)
        {
            return GetEnriched<EnrichedActivity>(options);
        }

        public Task<StreamResponse<EnrichedAggregatedActivity>> GetEnrichedAggregatedActivities(GetOptions options = null)
        {
            return GetEnriched<EnrichedAggregatedActivity>(options);
        }

        public Task<StreamResponse<EnrichedNotificationActivity>> GetEnrichedNotificationActivities(GetOptions options = null)
        {
            return GetEnriched<EnrichedNotificationActivity>(options);
        }

        public async Task FollowFeed(IStreamFeed feedToFollow, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            ValidateFeedFollow(feedToFollow);
            if (activityCopyLimit < 0)
                throw new ArgumentOutOfRangeException("activityCopyLimit", "Activity copy limit must be greater than or equal to 0");
            if (activityCopyLimit > StreamClient.ActivityCopyLimitMax)
                throw new ArgumentOutOfRangeException("activityCopyLimit", string.Format("Activity copy limit must be less than or equal to {0}", StreamClient.ActivityCopyLimitMax));

            var request = _client.BuildFeedRequest(this, "/following/", HttpMethod.POST);

            request.SetJsonBody(JsonConvert.SerializeObject(new
            {
                target = feedToFollow.FeedId,
                activity_copy_limit = activityCopyLimit,
            }));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public Task FollowFeed(string targetFeedSlug, string targetUserId, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            return FollowFeed(this._client.Feed(targetFeedSlug, targetUserId), activityCopyLimit);
        }

        public async Task UnfollowFeed(IStreamFeed feedToUnfollow, bool keepHistory = false)
        {
            ValidateFeedFollow(feedToUnfollow);

            var request = _client.BuildFeedRequest(this, "/following/" + feedToUnfollow.FeedId + "/", HttpMethod.DELETE);
            request.AddQueryParameter("keep_history", keepHistory.ToString());

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public Task UnfollowFeed(string targetFeedSlug, string targetUserId, bool keepHistory = false)
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

            var request = _client.BuildFeedRequest(this, "/followers/", HttpMethod.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", String.Join(",", filterBy));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return JsonConvert.DeserializeObject<FollowersResponse>(response.Content).results;
        }

        public async Task<IEnumerable<Follower>> Following(int offset = 0, int limit = 25, string[] filterBy = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Offset must be greater than or equal to zero");
            if (limit < 0)
                throw new ArgumentOutOfRangeException("limit", "Limit must be greater than or equal to zero");

            var request = _client.BuildFeedRequest(this, "/following/", HttpMethod.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", string.Join(",", filterBy));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return JsonConvert.DeserializeObject<FollowersResponse>(response.Content).results;
        }

        private void ValidateFeedFollow(IStreamFeed feed)
        {
            if (feed == null)
                throw new ArgumentNullException("feed", "Must have a feed to follow/unfollow");
            if (((StreamFeed)feed).FeedId == this.FeedId)
                throw new ArgumentException("Cannot follow/unfollow myself");
        }

    }
}
