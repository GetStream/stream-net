using RestSharp;

namespace Stream
{
    public class GetOptions
    {
        const int DefaultOffset = 0;
        const int DefaultLimit = 20;

        int _offset = DefaultOffset;
        int _limit = DefaultLimit;

        FeedFilter _filter = null;
        ActivityMarker _marker = null;

        string _ranking = null;
        
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

        public GetOptions WithRanking(string rankingSlug)
        {
            _ranking = rankingSlug;
            return this;
        }

        internal void Apply(RestRequest request)
        {
            // offset and and limit
            request.AddQueryParameter("offset", _offset.ToString());
            request.AddQueryParameter("limit", _limit.ToString());

            // add ranking
            if (!string.IsNullOrWhiteSpace(_ranking))
                request.AddQueryParameter("ranking", _ranking.ToString());

            // filter if needed
            if (_filter != null)
                _filter.Apply(request);

            // marker if needed
            if (_marker != null)
                _marker.Apply(request);
        }
        
        public static GetOptions Default
        {
            get
            {
                return new GetOptions()
                {
                    _offset = DefaultOffset,
                    _limit = DefaultLimit
                };
            }
        }
    }
}
