using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class BatchOperations
    {
        private const int CopyLimitDefault = 300;

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
            var request = _client.BuildRequest("feed/add_to_many", RestSharp.Method.POST);
            request.AddJsonBody(new
            {
                activity = activity,
                feeds = feedIds
            });

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        public async Task FollowMany(IEnumerable<Follow> follows, int activityCopyLimit = CopyLimitDefault)
        {
            var request = _client.BuildRequest("follow_many", RestSharp.Method.POST);
            request.AddJsonBody(follows);

            if (activityCopyLimit != CopyLimitDefault)
                request.AddQueryParameter("activity_copy_limit", activityCopyLimit.ToString());

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }
    }
}
