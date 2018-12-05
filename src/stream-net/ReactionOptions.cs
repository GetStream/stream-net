using Stream.Rest;
using System.Collections.Generic;

namespace Stream
{
    public class ReactionOption
    {
        private OpType Type { get; set; }
        private enum OpType
        {
            own,
            recent,
            counts,
        }

        readonly List<OpType> _ops;

        private ReactionOption()
        {
            _ops = new List<OpType>();
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
                }
            });
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
    }
}
