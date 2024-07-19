using Stream.Rest;
using Stream.Utils;
using System.Collections.Generic;

namespace Stream.Models
{
    public class GetOptions
    {
        private const int DefaultOffset = 0;
        private const int DefaultLimit = 20;
        private int _offset = DefaultOffset;
        private int _limit = DefaultLimit;
        private FeedFilter _filter;
        private ActivityMarker _marker = null;
        private ReactionOption _reaction = null;
        private string _ranking = null;
        private string _session = null;
        private string _endpoint = null;
        private string _feed_slug = null;
        private string _user_id = null;
        private string _ranking_vars = null;
        private bool _score_vars = false;
        private string _discard_actors = null;
        private string _discard_actors_sep = null;
        private string _moderation_template = null;

        private IDictionary<string, string> _custom = null;

        public GetOptions WithOffset(int offset)
        {
            _offset = offset;
            return this;
        }

        public GetOptions WithLimit(int limit)
        {
            _limit = limit;
            return this;
        }

        public GetOptions WithFilter(FeedFilter filter)
        {
            _filter = filter;
            return this;
        }

        public GetOptions WithMarker(ActivityMarker marker)
        {
            _marker = marker;
            return this;
        }

        public GetOptions WithReaction(ReactionOption reactions)
        {
            _reaction = reactions;
            return this;
        }

        public GetOptions WithRanking(string rankingSlug)
        {
            _ranking = rankingSlug;
            return this;
        }

        public GetOptions WithScoreVars()
        {
            _score_vars = true;
            return this;
        }

        public GetOptions WithRankingVars(IDictionary<string, object> rankingVars)
        {
            _ranking_vars = StreamJsonConverter.SerializeObject(rankingVars);
            return this;
        }

        public GetOptions WithModerationTemplate(String template)
        {
            _moderation_template = template;
            return this;
        }

        public GetOptions WithSession(string session)
        {
            _session = session;
            return this;
        }

        public GetOptions WithEndpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }

        public GetOptions WithFeedSlug(string feedSlug)
        {
            _feed_slug = feedSlug;
            return this;
        }

        public GetOptions WithUserId(string userId)
        {
            _user_id = userId;
            return this;
        }

        public GetOptions WithCustom(string key, string value)
        {
            if (_custom == null)
            {
                _custom = new Dictionary<string, string>();
            }

            _custom.Add(key, value);
            return this;
        }

        public GetOptions DiscardActors(List<string> actors, string separator = ",")
        {
            if (separator != ",")
            {
                _discard_actors_sep = separator;
            }

            _discard_actors = string.Join(separator, actors);
            return this;
        }

        internal void Apply(RestRequest request)
        {
            request.AddQueryParameter("offset", _offset.ToString());
            request.AddQueryParameter("limit", _limit.ToString());

            if (!string.IsNullOrWhiteSpace(_ranking))
                request.AddQueryParameter("ranking", _ranking);

            if (!string.IsNullOrWhiteSpace(_session))
                request.AddQueryParameter("session", _session);

            if (!string.IsNullOrWhiteSpace(_endpoint))
                request.AddQueryParameter("endpoint", _endpoint);

            if (!string.IsNullOrWhiteSpace(_feed_slug))
                request.AddQueryParameter("feed_slug", _feed_slug);

            if (!string.IsNullOrWhiteSpace(_user_id))
                request.AddQueryParameter("user_id", _user_id);

            if (!string.IsNullOrWhiteSpace(_ranking_vars))
                request.AddQueryParameter("ranking_vars", _ranking_vars);

            if (!string.IsNullOrWhiteSpace(_moderation_template))
                request.AddQueryParameter("moderation_template", _moderation_template);

            if (_score_vars)
                request.AddQueryParameter("withScoreVars", "true");

            if (!string.IsNullOrWhiteSpace(_discard_actors_sep))
                request.AddQueryParameter("discard_actors_sep", _discard_actors_sep);

            if (!string.IsNullOrWhiteSpace(_discard_actors))
                request.AddQueryParameter("discard_actors", _discard_actors);

            if (_custom != null)
            {
                foreach (KeyValuePair<string, string> kvp in _custom)
                    request.AddQueryParameter(kvp.Key, kvp.Value);
            }

            _filter?.Apply(request);
            _marker?.Apply(request);
            _reaction?.Apply(request);
        }

        public static GetOptions Default => new GetOptions();
    }
}
