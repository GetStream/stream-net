using Stream.Models;
using Stream.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream
{
    public class BatchOperations : IBatchOperations
    {
        private readonly StreamClient _client;

        internal BatchOperations(StreamClient client)
        {
            _client = client;
        }

        public async Task<ResponseBase> AddToManyAsync(Activity activity, IEnumerable<IStreamFeed> feeds)
        {
            return await AddToManyAsync(activity, feeds.Select(f => f.FeedId));
        }

        public async Task<ResponseBase> AddToManyAsync(Activity activity, IEnumerable<string> feedIds)
        {
            var request = _client.BuildAppRequest("feed/add_to_many/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new { activity = activity, feeds = feedIds }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> FollowManyAsync(IEnumerable<Follow> follows, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            if (activityCopyLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(activityCopyLimit), $"{nameof(activityCopyLimit)} must be >= 0");
            if (activityCopyLimit > StreamClient.ActivityCopyLimitMax)
                throw new ArgumentOutOfRangeException(nameof(activityCopyLimit), $"{nameof(activityCopyLimit)} must be less than or equal to {StreamClient.ActivityCopyLimitMax}");

            var request = _client.BuildAppRequest("follow_many/", HttpMethod.Post);

            request.AddQueryParameter("activity_copy_limit", activityCopyLimit.ToString());
            request.SetJsonBody(StreamJsonConverter.SerializeObject(follows));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<GenericGetResponse<Activity>> GetActivitiesByIdAsync(IEnumerable<string> ids)
            => await GetActivitiesAsync(ids, null);

        public async Task<GenericGetResponse<Activity>> GetActivitiesByForeignIdAsync(IEnumerable<ForeignIdTime> ids)
            => await GetActivitiesAsync(null, ids);

        private async Task<GenericGetResponse<Activity>> GetActivitiesAsync(IEnumerable<string> ids = null, IEnumerable<ForeignIdTime> foreignIdTimes = null)
        {
            var request = _client.BuildAppRequest("activities/", HttpMethod.Get);

            if (ids != null)
            {
                request.AddQueryParameter("ids", string.Join(",", ids));
            }

            if (foreignIdTimes != null)
            {
                request.AddQueryParameter("foreign_ids", string.Join(",", foreignIdTimes.Select(f => f.ForeignId)));
                request.AddQueryParameter("timestamps", string.Join(",", foreignIdTimes.Select(f =>
                        f.Time.ToString("s", System.Globalization.CultureInfo.InvariantCulture))));
            }

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<GenericGetResponse<Activity>>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<GenericGetResponse<EnrichedActivity>> GetEnrichedActivitiesAsync(IEnumerable<string> ids, GetOptions options = null)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException($"Activity {nameof(ids)} must be provided.", nameof(ids));

            return await GetEnrichedActivitiesAsync(ids, null, options);
        }

        public async Task<GenericGetResponse<EnrichedActivity>> GetEnrichedActivitiesAsync(IEnumerable<ForeignIdTime> foreignIdTimes, GetOptions options = null)
        {
            if (foreignIdTimes == null || !foreignIdTimes.Any())
                throw new ArgumentException($"{nameof(foreignIdTimes)} must be provided.", nameof(foreignIdTimes));

            return await GetEnrichedActivitiesAsync(null, foreignIdTimes, options);
        }

        private async Task<GenericGetResponse<EnrichedActivity>> GetEnrichedActivitiesAsync(IEnumerable<string> ids = null, IEnumerable<ForeignIdTime> foreignIdTimes = null, GetOptions options = null)
        {
            var request = _client.BuildAppRequest("enrich/activities/", HttpMethod.Get);

            if (ids != null && ids.Any())
            {
                request.AddQueryParameter("ids", string.Join(",", ids));
            }

            if (foreignIdTimes != null && foreignIdTimes.Any())
            {
                request.AddQueryParameter("foreign_ids", string.Join(",", foreignIdTimes.Select(f => f.ForeignId)));
                request.AddQueryParameter("timestamps", string.Join(",", foreignIdTimes.Select(f =>
                        f.Time.ToString("s", System.Globalization.CultureInfo.InvariantCulture))));
            }

            options?.Apply(request);

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
                return StreamJsonConverter.DeserializeObject<GenericGetResponse<EnrichedActivity>>(response.Content);
            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> UpdateActivitiesAsync(IEnumerable<Activity> activities)
        {
            var request = _client.BuildAppRequest("activities/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new { activities = activities }));

            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> ActivitiesPartialUpdateAsync(IEnumerable<ActivityPartialUpdateRequestObject> updates)
        {
            var request = _client.BuildAppRequest("activity/", HttpMethod.Post);
            request.SetJsonBody(StreamJsonConverter.SerializeObject(new { changes = updates }));
            var response = await _client.MakeRequestAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
                return StreamJsonConverter.DeserializeObject<ResponseBase>(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task<ResponseBase> UpdateActivityAsync(Activity activity)
        {
            return await UpdateActivitiesAsync(new[] { activity });
        }
    }
}
