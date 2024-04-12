using Stream.Rest;
using System.Collections.Generic;
using System.Linq;

namespace Stream.Models
{
    public class FeedFilter
    {
#pragma warning disable SA1300
        internal enum OpType
        {
            id_gte,
            id_gt,
            id_lte,
            id_lt,
            with_activity_data,
            discard_actors,
            discard_actors_sep,
        }
#pragma warning restore SA1300

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

        private readonly List<OpEntry> _ops = new List<OpEntry>();

        public FeedFilter WithActivityData()
        {
            _ops.Add(new OpEntry(OpType.with_activity_data, "true"));
            return this;
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

        public FeedFilter DiscardActors(string[] actors, string separator = ",")
        {
            if (separator != ",")
            {
                _ops.Add(new OpEntry(OpType.discard_actors_sep, separator));
            }

            _ops.Add(new OpEntry(OpType.discard_actors, string.Join(separator, actors)));
            return this;
        }
        internal void Apply(RestRequest request)
        {
            _ops.ForEach(op =>
            {
                request.AddQueryParameter(op.Type.ToString(), op.Value);
            });
        }

        internal bool IncludesActivityData => _ops.Any(x => x.Type == OpType.with_activity_data);

        public static FeedFilter Where() => new FeedFilter();
    }
}
