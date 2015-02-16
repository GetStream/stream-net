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
        readonly String _feedSlug;
        readonly String _userId;

        internal StreamFeed(StreamClient client, String feedSlug, String userId, String token)
        {
            if (!_feedRegex.IsMatch(feedSlug))
                throw new ArgumentException("Feed slug can only contain alphanumeric characters or underscores");
            if (!_userRegex.IsMatch(userId))
                throw new ArgumentException("User id can only contain alphanumeric characters, underscores or dashes");

            Token = token;
            _client = client;
            _feedSlug = feedSlug;
            _userId = userId;
            FeedTokenId = String.Format("{0}{1}", _feedSlug, _userId);
            UrlPath = String.Format("feed/{0}/{1}", _feedSlug, _userId);
        }

        internal String FeedTokenId { get; private set; }

        public String Token { get; private set; }

        public String UrlPath { get; private set; }        

        /// <summary>
        /// Add an activity to the feed
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>An activity with ID and Date supplied</returns>
        public async Task<Activity> AddActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity", "Must have an activity to add");
       
            var request = _client.BuildRequest(this, "/", Method.POST);
            request.AddParameter("application/json", activity.ToJson(this._client), ParameterType.RequestBody);          
            
            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return Activity.FromJson(response.Content);

            throw StreamException.FromResponse(response);
        }
      
        internal String ToActivitiesJson(IEnumerable<Activity> activities)
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
            if ((activities == null) || (activities.Count() == 0))
                throw new ArgumentNullException("activities", "Must have activities to add");

            var request = _client.BuildRequest(this, "/", Method.POST);
            request.AddParameter("application/json", ToActivitiesJson(activities), ParameterType.RequestBody);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return GetResults(response.Content);

            throw StreamException.FromResponse(response);
        }

        /// <summary>
        /// Remove an activity
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="foreignId"></param>
        /// <returns></returns>
        public async Task RemoveActivity(String activityId, bool foreignId = false)
        {
            var request = _client.BuildRequest(this, "/" + activityId + "/", Method.DELETE);
            if (foreignId)
                request.AddQueryParameter("foreign_id", "1");
            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        internal IEnumerable<Activity> GetResults(String json)
        {
            JObject obj = JObject.Parse(json);
            foreach(var prop in obj.Properties())
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

        public async Task<IEnumerable<Activity>> GetActivities(int offset = 0, int limit = 20, FeedFilter filter = null)
        {
            var request = _client.BuildRequest(this, "/", Method.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            // filter if needed
            if (filter != null)
                filter.Apply(request);

            var response = await _client.MakeRequest(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return GetResults(response.Content);
                        
            throw StreamException.FromResponse(response);
        }

        public async Task FollowFeed(String targetFeedSlug, String targetUserId)
        {
            var targetFeedId = String.Format("{0}:{1}", targetFeedSlug, targetUserId);
            var targetFeed = this._client.Feed(targetFeedSlug, targetUserId);            

            var request = _client.BuildRequest(this, "/follows/", Method.POST);
            request.AddJsonBody(new
            {
                target = targetFeedId,
                target_token = targetFeed.Token
            });

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw StreamException.FromResponse(response);
        }

        public async Task UnfollowFeed(String targetFeedSlug, String targetUserId)
        {
            var targetFeedId = String.Format("{0}:{1}", targetFeedSlug, targetUserId);
            var request = _client.BuildRequest(this, "/follows/" + targetFeedId + "/", Method.DELETE);
            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }

        internal class FollowersResponse
        {
            public IEnumerable<Follower> results { get; set; }
        }

        public async Task<IEnumerable<Follower>> Followers(int offset = 0, int limit = 25, String[] filterBy = null)
        {
            var request = _client.BuildRequest(this, "/followers/", Method.GET);
            request.AddQueryParameter("offset", offset.ToString());
            request.AddQueryParameter("limit", limit.ToString());

            if (filterBy.SafeCount() > 0)
                request.AddQueryParameter("filter", String.Join(",",filterBy));

            var response = await _client.MakeRequest(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);

            return JsonConvert.DeserializeObject<FollowersResponse>(response.Content).results;
        }

        public async Task<IEnumerable<Follower>> Following(int offset = 0, int limit = 25, String[] filterBy = null)
        {
            var request = _client.BuildRequest(this, "/following/", Method.GET);
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
            var request = _client.BuildRequest(this, "/", Method.DELETE);
            var response = await _client.MakeRequest(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw StreamException.FromResponse(response);
        }
    }
}
