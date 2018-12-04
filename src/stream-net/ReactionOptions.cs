using Stream.Rest;
using System.Collections.Generic;

namespace Stream
{
    public class ReactionOption
    {
        private OpType Type {get; set;}
        private enum OpType
        {
            own,
            recent,
            counts,
        }

        private ReactionOption(OpType type)
        {
            Type = type;
        }

        internal void Apply(RestRequest request)
        {
            switch (Type)
            {
                case OpType.own: request.AddQueryParameter("withOwnReactions", "true"); break;
                case OpType.recent: request.AddQueryParameter("withRecentReactions", "true"); break;
                case OpType.counts: request.AddQueryParameter("withReactionCounts", "true"); break;
            }
        }

        public static ReactionOption Own()
        {
            return new ReactionOption(OpType.own);
        }

        public static ReactionOption Recent()
        {
            return new ReactionOption(OpType.recent);
        }

        public static ReactionOption Counts()
        {
            return new ReactionOption(OpType.counts);
        }
    }
}
