using Newtonsoft.Json;
using Stream.Rest;
using System;
using System.Collections.Generic;
using ReactionFilter = Stream.Models.FeedFilter;

namespace Stream.Models
{
    public class Reaction
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ActivityId { get; set; }
        public string UserId { get; set; }
        public GenericData User { get; set; }

        public IDictionary<string, object> Data { get; set; }
        public IEnumerable<string> TargetFeeds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "parent")]
        public string ParentId { get; set; }

        public Dictionary<string, Reaction[]> LatestChildren { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "children_counts")]
        public Dictionary<string, int> ChildrenCounters { get; set; }
    }

    public class ReactionsWithActivity
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "results")]
        public IEnumerable<Reaction> Reactions { get; set; }

        public EnrichedActivity Activity { get; set; }
    }

    internal class ReactionsFilterResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "results")]
        public IEnumerable<Reaction> Reactions { get; set; }
    }

    public class ReactionFiltering
    {
        private const int DefaultLimit = 10;
        private int _limit = DefaultLimit;
        private ReactionFilter _filter = null;

        public ReactionFiltering WithLimit(int limit)
        {
            _limit = limit;
            return this;
        }

        public ReactionFiltering WithFilter(ReactionFilter filter)
        {
            _filter = filter;
            return this;
        }

        internal ReactionFiltering WithActivityData()
        {
            _filter = _filter == null ? ReactionFilter.Where().WithActivityData() : _filter.WithActivityData();
            return this;
        }

        internal void Apply(RestRequest request)
        {
            request.AddQueryParameter("limit", _limit.ToString());
            _filter?.Apply(request);
        }

        internal bool IncludesActivityData => _filter.IncludesActivityData;

        public static ReactionFiltering Default => new ReactionFiltering().WithLimit(DefaultLimit);
    }

    public class ReactionPagination
    {
        private string _kind;
        private string _lookup_attr;
        private string _lookup_value;

        public ReactionPagination ActivityId(string activityId)
        {
            _lookup_attr = "activity_id";
            _lookup_value = activityId;
            return this;
        }

        public ReactionPagination ReactionId(string reactionId)
        {
            _lookup_attr = "reaction_id";
            _lookup_value = reactionId;
            return this;
        }

        public ReactionPagination UserId(string userId)
        {
            _lookup_attr = "user_id";
            _lookup_value = userId;
            return this;
        }

        public ReactionPagination Kind(string kind)
        {
            _kind = kind;
            return this;
        }

        public static ReactionPagination By { get => new ReactionPagination(); }

        public string GetPath()
            => _kind == null ? $"{_lookup_attr}/{_lookup_value}/" : $"{_lookup_attr}/{_lookup_value}/{_kind}/";
    }
}
