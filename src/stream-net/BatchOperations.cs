using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stream
{
    public class BatchOperations
    {
        readonly StreamClient _client;

        internal BatchOperations(StreamClient client)
        {
            _client = client;
        }

        public Task AddToMany(Activity activity, IEnumerable<StreamFeed> feeds)
        {
            return AddToMany(activity, feeds.Select(f => f.FeedId));
        }

        public async Task AddToMany(Activity activity, IEnumerable<string> feedIds)
        {
            var request = _client.BuildAppRequest("feed/add_to_many/", RestSharp.Method.POST);
            request.AddParameter("application/json",
                "{" + string.Format("\"activity\": {0}, \"feeds\": {1}", activity.ToJson(this._client), JsonConvert.SerializeObject(feedIds)) + "}"
                , ParameterType.RequestBody);
            _client.SignRequest(request);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task FollowMany(IEnumerable<Follow> follows, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            if (activityCopyLimit < 1)
                throw new ArgumentOutOfRangeException("activityCopyLimit", "Activity copy limit must be greater than 0");
            if (activityCopyLimit > StreamClient.ActivityCopyLimitMax)
                throw new ArgumentOutOfRangeException("activityCopyLimit", string.Format("Activity copy limit must be less than or equal to {0}", StreamClient.ActivityCopyLimitMax));

            var request = _client.BuildAppRequest("follow_many/", RestSharp.Method.POST);

            request.AddQueryParameter("activity_copy_limit", activityCopyLimit.ToString());
            request.AddJsonBody(from f in follows
                                select new
                                {
                                    source = f.Source,
                                    target = f.Target
                                });

            _client.SignRequest(request);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }
    }
}
