﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stream
{
    public class ForeignIDTime
    {
        public string ForeignID { get; set; }
        public DateTime Time { get; set; }

        public ForeignIDTime(string foreignID, DateTime time)
        {
            ForeignID = foreignID;
            Time = time;
        }
    }

    public class ActivityPartialUpdateRequestObject
    {
        public string ID { get; set; }

        public ForeignIDTime ForeignIDTime { get; set; }

        public GenericData Set { get; set; }

        public IEnumerable<string> Unset { get; set; }

        public JObject ToJObject()
        {
            var result = new JObject();

            if (ID != null)
            {
                result.Add(new JProperty("id", ID));
            }
            else
            {
                result.Add(new JProperty("foreign_id", ForeignIDTime.ForeignID));
                result.Add(new JProperty("time", ForeignIDTime.Time.ToString("s", System.Globalization.CultureInfo.InvariantCulture)));
            }

            var setObj = new JObject();
            if (Set != null)
            {
                Set.AddToJObject(ref setObj);
            }
            result.Add("set", setObj);

            result.Add(new JProperty("unset", Unset != null ? Unset : new string[] { }));
            return result;
        }
    }

    public class BatchOperations : IBatchOperations
    {
        readonly StreamClient _client;

        internal BatchOperations(StreamClient client)
        {
            _client = client;
        }

        public Task AddToMany(Activity activity, IEnumerable<IStreamFeed> feeds)
        {
            return AddToMany(activity, feeds.Select(f => f.FeedId));
        }

        public async Task AddToMany(Activity activity, IEnumerable<string> feedIds)
        {
            var request = _client.BuildAppRequest("feed/add_to_many/", HttpMethod.POST);
            request.SetJsonBody(
                "{" + string.Format("\"activity\": {0}, \"feeds\": {1}", activity.ToJson(this._client), JsonConvert.SerializeObject(feedIds)) + "}"
            );

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task FollowMany(IEnumerable<Follow> follows, int activityCopyLimit = StreamClient.ActivityCopyLimitDefault)
        {
            if (activityCopyLimit < 0)
                throw new ArgumentOutOfRangeException("activityCopyLimit", "Activity copy limit must be greater than or equal to 0");
            if (activityCopyLimit > StreamClient.ActivityCopyLimitMax)
                throw new ArgumentOutOfRangeException("activityCopyLimit", string.Format("Activity copy limit must be less than or equal to {0}", StreamClient.ActivityCopyLimitMax));

            var request = _client.BuildAppRequest("follow_many/", HttpMethod.POST);

            request.AddQueryParameter("activity_copy_limit", activityCopyLimit.ToString());
            request.SetJsonBody(JsonConvert.SerializeObject(from f in follows
                                                            select new
                                                            {
                                                                source = f.Source,
                                                                target = f.Target
                                                            }));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task<IEnumerable<Activity>> GetActivities(IEnumerable<string> ids = null, IEnumerable<ForeignIDTime> foreignIDTimes = null)
        {
            if (ids == null && foreignIDTimes == null)
                throw new ArgumentException("one of the parameters ids or foreignIdTimes must be provided and not null", "ids, foreignIDTimes");
            if (ids != null && foreignIDTimes != null)
                throw new ArgumentException("at most one of the parameters ids or foreignIdTimes must be provided", "ids, foreignIDTimes");

            var request = _client.BuildAppRequest("activities/", HttpMethod.GET);

            if (ids != null)
            {
                request.AddQueryParameter("ids", string.Join(",", ids));
            }

            if (foreignIDTimes != null)
            {
                request.AddQueryParameter("foreign_ids", string.Join(",", foreignIDTimes.Select(f => f.ForeignID)));
                request.AddQueryParameter("timestamps", string.Join(",", foreignIDTimes.Select(f =>
                        f.Time.ToString("s", System.Globalization.CultureInfo.InvariantCulture))));
            }

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Activity.GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        public async Task UpdateActivities(IEnumerable<Activity> activities)
        {
            var request = _client.BuildAppRequest("activities/", HttpMethod.POST);
            request.SetJsonBody(Activity.ToActivitiesJson(activities, this._client));

            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task ActivitiesPartialUpdate(IEnumerable<ActivityPartialUpdateRequestObject> updates)
        {
            var request = this._client.BuildAppRequest("activity/", HttpMethod.POST);

            var requestData = new Dictionary<string, object>(){
                {"changes", updates.Select(x => x.ToJObject())}
            };

            request.SetJsonBody(JsonConvert.SerializeObject(requestData));
            var response = await this._client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task UpdateActivity(Activity activity)
        {
            await UpdateActivities(new Activity[] { activity });
        }

    }
}
