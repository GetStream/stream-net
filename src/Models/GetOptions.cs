using Stream.Rest;

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

        public GetOptions WithSession(string session)
        {
            _session = session;
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

            _filter?.Apply(request);
            _marker?.Apply(request);
            _reaction?.Apply(request);
        }

        public static GetOptions Default => new GetOptions();
    }
}
