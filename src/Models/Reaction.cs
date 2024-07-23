using Newtonsoft.Json;
using Stream.Rest;
using System;
using System.Collections.Generic;
using ReactionFilter = Stream.Models.FeedFilter;
using Newtonsoft.Json.Linq;

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
        public string ModerationTemplate { get; set; }
        public GenericData User { get; set; }

        public IDictionary<string, object> Data { get; set; }
        public IEnumerable<string> TargetFeeds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "parent")]
        public string ParentId { get; set; }

        public Dictionary<string, Reaction[]> LatestChildren { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "children_counts")]
        public Dictionary<string, int> ChildrenCounters { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string Ref() => $"SR:{Id}";

        public Dictionary<string, object> Moderation { get; set; }
        public ModerationResponse GetModerationResponse()
        {
              var key = "response";
              if (Moderation != null && Moderation.ContainsKey(key))
              {
                  return ((JObject)Moderation[key]).ToObject<ModerationResponse>();
              }
              else
              {
                  throw new KeyNotFoundException($"Key '{key}' not found in moderation dictionary.");
              }
        }
    }

    public class ReactionsWithActivity : GenericGetResponse<Reaction>
    {
        public EnrichedActivity Activity { get; set; }
    }

    internal class ReactionsFilterResponse : GenericGetResponse<Reaction>
    {
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
        private string _lookupAttr;
        private string _lookupValue;

        public ReactionPagination ActivityId(string activityId)
        {
            _lookupAttr = "activity_id";
            _lookupValue = activityId;
            return this;
        }

        public ReactionPagination ReactionId(string reactionId)
        {
            _lookupAttr = "reaction_id";
            _lookupValue = reactionId;
            return this;
        }

        public ReactionPagination UserId(string userId)
        {
            _lookupAttr = "user_id";
            _lookupValue = userId;
            return this;
        }

        public ReactionPagination Kind(string kind)
        {
            _kind = kind;
            return this;
        }

        public static ReactionPagination By { get => new ReactionPagination(); }

        public string GetPath()
            => _kind == null ? $"{_lookupAttr}/{_lookupValue}/" : $"{_lookupAttr}/{_lookupValue}/{_kind}/";
    }
}