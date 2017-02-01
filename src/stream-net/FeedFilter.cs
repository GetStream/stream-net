using Stream.Rest;
using System.Collections.Generic;

namespace Stream
{
    public class FeedFilter
    {
        internal enum OpType
        {
            id_gte,
            id_gt,
            id_lte,
            id_lt
        }

        internal class OpEntry
        {
            internal OpType Type { get; set; }

            internal string Value { get; set; }

            internal OpEntry(OpType type, string value) 
            {
                Type = type;
                Value = value;
            }
        }

        readonly List<OpEntry> _ops = new List<OpEntry>();

        private FeedFilter()
        {
        }

        public FeedFilter IdGreaterThan(string id)
        {
            _ops.Add(new OpEntry(OpType.id_gt, id));
            return this;
        }

        public FeedFilter IdGreaterThanEqual(string id)
        {
            _ops.Add(new OpEntry(OpType.id_gte, id));
            return this;
        }

        public FeedFilter IdLessThan(string id)
        {
            _ops.Add(new OpEntry(OpType.id_lt, id));
            return this;
        }

        public FeedFilter IdLessThanEqual(string id)
        {
            _ops.Add(new OpEntry(OpType.id_lte, id));
            return this;
        }

        internal void Apply(RestRequest request)
        {
            _ops.ForEach((op) =>
            {
                request.AddQueryParameter(op.Type.ToString(), op.Value);
            });
        }

        #region starts

        public static FeedFilter Where()
        {
            return new FeedFilter();
        }

        #endregion
    }
}
