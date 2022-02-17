using Stream.Models;
using Stream.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamFeed : IStreamFeed
    {
        private static readonly Regex _feedRegex = new Regex(@"^\w+$", RegexOptions.Compiled);
        private static readonly Regex _userRegex = new Regex(@"^[-\w]+$", RegexOptions.Compiled);
        private readonly StreamClient _client;
        private readonly string _feedSlug;
        private readonly string _userId;

        internal StreamFeed(StreamClient client, string feedSlug, string userId)
        {
            if (!_feedRegex.IsMatch(feedSlug))
                throw new ArgumentException("Feed slug can only contain alphanumeric characters or underscores", nameof(feedSlug));
            if (!_userRegex.IsMatch(userId))
                throw new ArgumentException("User id can only contain alphanumeric characters, underscores or dashes", nameof(userId));

            _client = client;
            _feedSlug = feedSlug;
            _userId = userId;
            FeedId = $"{_feedSlug}:{_userId}";
            UrlPath = $"feed/{_feedSlug}/{_userId}";
            EnrichedPath = "enrich/" + UrlPath;
        }

        public string FeedId { get; }
        public string UrlPath { get; }
        public string EnrichedPath { get; }

        /// <summary>
        /// Add an activity to the feed
        /// </summary>
        /// <returns>An activity with ID and Date supplied</returns>
        public async Task<Activity> AddActivityAsync(Activity activity)
        {
            var request = _client.BuildFeedRequest(this, "/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(activity));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<Activity>(response.Content);

            throw StreamException.FromResponse(response);
        }

        /// <summary>
        /// Add a list of activities
        /// </summary>
        public async Task<AddActivitiesResponse> AddActivitiesAsync(IEnumerable<Activity> activities)
        {
            var request = _client.BuildFeedRequest(this, "/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new { activities }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<AddActivitiesResponse>(response.Content);

            throw StreamException.FromResponse(response);
        }

        /// <summary>
        /// Update an activity to the feed
        /// </summary>
        public async Task<ResponseBase> UpdateActivityAsync(Activity activity)
        {
            return await UpdateActivitiesAsync(new[] { activity });
        }

        /// <summary>
        /// Update a list of activities, Maximum length is 100
        /// </summary>
        public async Task<ResponseBase> UpdateActivitiesAsync(IEnumerable<Activity> activities)
        {
            var request = _client.BuildActivitiesRequest();
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new { activities }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        /// <summary>
        /// Remove an activity
        /// </summary>
        public async Task<ResponseBase> RemoveActivityAsync(string activityId, bool foreignId = false)
        {
            var request = _client.BuildFeedRequest(this, "/" + activityId + "/", HttpMethod.Delete);
            if (foreignId)
                request.AddQueryParameter("foreign_id", "1");
            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<GenericGetResponse<Activity>> GetActivitiesAsync(int offset = 0, int limit = 20, FeedFilter filter = null, ActivityMarker marker = null)
        {
            var request = _client.BuildFeedRequest(this, "/", HttpMethod.Get);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            filter?.Apply(request);
            marker?.Apply(request);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<GenericGetResponse<Activity>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<UpdateToTargetsResponse> UpdateActivityToTargetsAsync(ForeignIdTime foreignIdTime,
            IEnumerable<string> adds = null,
            IEnumerable<string> newTargets = null,
            IEnumerable<string> removed = null)
        {
            var payload = new
            {
                foreign_id = foreignIdTime.ForeignId,
                time = foreignIdTime.Time,
                added_targets = adds,
                new_targets = newTargets,
                removed_targets = removed,
            };

            var endpoint = $"feed_targets/{_feedSlug}/{_userId}/activity_to_targets/";
            var request = _client.BuildAppRequest(endpoint, HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(payload));
            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<UpdateToTargetsResponse>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<GenericGetResponse<Activity>> GetFlatActivitiesAsync(GetOptions options = null)
            => await GetActivitiesAsync<GenericGetResponse<Activity>>(options);

        public async Task<GenericGetResponse<AggregateActivity>> GetAggregateActivitiesAsync(GetOptions options = null)
            => await GetActivitiesAsync<GenericGetResponse<AggregateActivity>>(options);

        public async Task<NotificationGetResponse> GetNotificationActivities(GetOptions options = null)
            => await GetActivitiesAsync<NotificationGetResponse>(options);

        private async Task<T> GetActivitiesAsync<T>(GetOptions options = null)
        {
            options = options ?? GetOptions.Default;
            var request = _client.BuildFeedRequest(this, "/", HttpMethod.Get);
            options.Apply(request);

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<T>(response.Content);
            throw StreamException.FromResponse(response);
        }

        public async Task<GenericGetResponse<EnrichedActivity>> GetEnrichedFlatActivitiesAsync(GetOptions options = null)
            => await GetEnrichedActivitiesAsync<EnrichedActivity>(options);

        public async Task<GenericGetResponse<EnrichedAggregateActivity>> GetEnrichedAggregatedActivitiesAsync(GetOptions options = null)
            => await GetEnrichedActivitiesAsync<EnrichedAggregateActivity>(options);

        public async Task<GenericGetResponse<EnrichedNotificationActivity>> GetEnrichedNotificationActivitiesAsync(GetOptions options = null)
            => await GetEnrichedActivitiesAsync<EnrichedNotificationActivity>(options);

        private async Task<GenericGetResponse<T>> GetEnrichedActivitiesAsync<T>(GetOptions options = null)
        {
            options = options ?? GetOptions.Default;
            var request = _client.BuildEnrichedFeedRequest(this, "/", HttpMethod.Get);
            options.Apply(request);

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<GenericGetResponse<T>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> FollowFeedAsync(string targetFeedSlug, string targetUserId, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            return await FollowFeedAsync(_client.Feed(targetFeedSlug, targetUserId), activityCopyLimit);
        }

        public async Task<ResponseBase> FollowFeedAsync(IStreamFeed feedToFollow, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            ValidateFeedFollow(feedToFollow);
            if (activityCopyLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(activityCopyLimit), "Activity copy limit must be greater than or equal to 0");
            if (activityCopyLimit > StreamClient.ActivityCopyLimitMax)
                throw new ArgumentOutOfRangeException(nameof(activityCopyLimit), string.Format("Activity copy limit must be less than or equal to {0}", StreamClient.ActivityCopyLimitMax));

            var request = _client.BuildFeedRequest(this, "/following/", HttpMethod.Post);

            request.SetJsonBody(StreamJsonConverter.SerializeObject(new
            {
                target = feedToFollow.FeedId,
                activity_copy_limit = activityCopyLimit,
            }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> UnfollowFeedAsync(string targetFeedSlug, string targetUserId, bool keepHistory = false)
        {
            return await UnfollowFeedAsync(_client.Feed(targetFeedSlug, targetUserId), keepHistory);
        }

        public async Task<ResponseBase> UnfollowFeedAsync(IStreamFeed feedToUnfollow, bool keepHistory = false)
        {
            ValidateFeedFollow(feedToUnfollow);

            var request = _client.BuildFeedRequest(this, "/following/" + feedToUnfollow.FeedId + "/", HttpMethod.Delete);
            request.AddQueryParameter("keep_history", keepHistory.ToString());

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        private void ValidateFeedFollow(IStreamFeed feed)
        {
            if (feed == null)
                throw new ArgumentNullException(nameof(feed), "Must have a feed to follow/unfollow");
            if (feed.FeedId == this.FeedId)
                throw new ArgumentException("Cannot follow/unfollow myself");
        }

        public async Task<GenericGetResponse<Follower>> FollowersAsync(int offset = 0, int limit = 25, IEnumerable<string> filterBy = null)
        {
            var request = _client.BuildFeedRequest(this, "/followers/", HttpMethod.Get);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", string.Join(",", filterBy));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<GenericGetResponse<Follower>>(response.Content);
        }

        public async Task<GenericGetResponse<Follower>> FollowingAsync(int offset = 0, int limit = 25, IEnumerable<string> filterBy = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to zero");
            if (limit < 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than or equal to zero");

            var request = _client.BuildFeedRequest(this, "/following/", HttpMethod.Get);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", string.Join(",", filterBy));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<GenericGetResponse<Follower>>(response.Content);
        }

        public async Task<FollowStatsResponse> FollowStatsAsync(IEnumerable<string> followersSlugs = null, IEnumerable<string> followingSlugs = null)
        {
            var request = _client.BuildAppRequest("stats/follow/", HttpMethod.Get);
            request.AddQueryParameter("followers", this.FeedId);
            request.AddQueryParameter("following", this.FeedId);

            if (followersSlugs != null)
            {
                request.AddQueryParameter("followers_slugs", string.Join(",", followersSlugs));
            }

            if (followingSlugs != null)
            {
                request.AddQueryParameter("following_slugs", string.Join(",", followingSlugs));
            }

            var response = await _client.MakeRequestAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return StreamJsonConverter.DeserializeObject<FollowStatsResponse>(response.Content);
        }
    }
}
