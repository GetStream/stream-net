using Stream.Rest;
using System.Collections.Generic;

namespace Stream.Models
{
    public class ReactionOption
    {
        private int? _recentLimit;

        private OpType Type { get; set; }

#pragma warning disable SA1300
        private enum OpType
        {
            own,
            recent,
            counts,
            ownChildren,
        }
#pragma warning restore SA1300

        private readonly List<OpType> _ops;
        private readonly List<string> _kindFilters;

        private ReactionOption()
        {
            _ops = new List<OpType>();
            _kindFilters = new List<string>();
        }

        internal void Apply(RestRequest request)
        {
            _ops.ForEach(op =>
            {
                switch (op)
                {
                    case OpType.own: request.AddQueryParameter("withOwnReactions", "true"); break;
                    case OpType.recent: request.AddQueryParameter("withRecentReactions", "true"); break;
                    case OpType.counts: request.AddQueryParameter("withReactionCounts", "true"); break;
                    case OpType.ownChildren: request.AddQueryParameter("withOwnChildren", "true"); break;
                }
            });
            if (_recentLimit.HasValue)
                request.AddQueryParameter("recentReactionsLimit", _recentLimit.ToString());

            if (_kindFilters.Count != 0)
                request.AddQueryParameter("reactionKindsFilter", string.Join(",", _kindFilters));
        }

        public static ReactionOption With()
        {
            return new ReactionOption();
        }

        public ReactionOption Own()
        {
            _ops.Add(OpType.own);
            return this;
        }

        public ReactionOption Recent()
        {
            _ops.Add(OpType.recent);
            return this;
        }

        public ReactionOption Counts()
        {
            _ops.Add(OpType.counts);
            return this;
        }

        public ReactionOption OwnChildren()
        {
            _ops.Add(OpType.ownChildren);
            return this;
        }

        public ReactionOption RecentLimit(int value)
        {
            _recentLimit = value;
            return this;
        }

        public ReactionOption KindFilter(string value)
        {
            _kindFilters.Add(value);
            return this;
        }
    }
}
