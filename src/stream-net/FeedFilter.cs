using RestSharp;
using System;
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
            internal String Value { get; set; }

            internal OpEntry(OpType type, String value) 
            {
                Type = type;
                Value = value;
            }
        }

        readonly List<OpEntry> _ops = new List<OpEntry>();

        private FeedFilter()
        {
        }

        public FeedFilter IdGreaterThan(String id)
        {
            _ops.Add(new OpEntry(OpType.id_gt, id));
            return this;
        }

        public FeedFilter IdGreaterThanEqual(String id)
        {
            _ops.Add(new OpEntry(OpType.id_gte, id));
            return this;
        }

        public FeedFilter IdLessThan(String id)
        {
            _ops.Add(new OpEntry(OpType.id_lt, id));
            return this;
        }

        public FeedFilter IdLessThanEqual(String id)
        {
            _ops.Add(new OpEntry(OpType.id_lte, id));
            return this;
        }

        internal void Apply(IRestRequest request)
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
