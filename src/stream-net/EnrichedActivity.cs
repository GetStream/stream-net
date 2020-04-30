using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetStream
{
    public class EnrichedActivity
    {
        private const string Field_Id = "id";
        private const string Field_Actor = "actor";
        private const string Field_Verb = "verb";
        private const string Field_Object = "object";
        private const string Field_ForeignId = "foreign_id";
        private const string Field_To = "to";
        private const string Field_Target = "target";
        private const string Field_Time = "time";
        private const string Field_Activities = "activities";
        private const string Field_ActorCount = "actor_count";
        private const string Field_IsRead = "is_read";
        private const string Field_IsSeen = "is_seen";
        private const string Field_CreatedAt = "created_at";
        private const string Field_UpdatedAt = "updated_at";
        private const string Field_Group = "group";
        private const string Field_OwnReactions = "own_reactions";
        private const string Field_LatestReactions = "latest_reactions";
        private const string Field_ReactionCounts = "reaction_counts";

        readonly IDictionary<string, JToken> _data = new Dictionary<string, JToken>();

        public string Id { get; private set; }

        public EnrichableField Actor { get; private set; }

        public EnrichableField Verb { get; private set; }

        public EnrichableField Object { get; private set; }

        public EnrichableField Target { get; private set; }

        public DateTime? Time { get; private set; }

        public EnrichableField ForeignId { get; private set; }

        public IList<string> To { get; private set; }

        public IDictionary<string, IEnumerable<Reaction>> OwnReactions { get; private set; }

        public IDictionary<string, IEnumerable<Reaction>> LatestReactions { get; private set; }

        public IDictionary<string, int> ReactionCounts { get; private set; }

        public T GetData<T>(string name)
        {
            if (_data.ContainsKey(name))
            {
                var token = _data[name];

                // if we are asking for a custom type.. but the JToken is a string
                if ((!typeof(T).IsBuiltInType()) &&
                    (token.Type == JTokenType.String))
                {
                    try // to deserialize the string into the object
                    {
                        return JsonConvert.DeserializeObject<T>(token.ToString());
                    }
                    catch (Exception)
                    {
                        // we'll eat this exception since the next one most likely toss
                    }
                }

                return _data[name].ToObject<T>();
            }
            return default(T);
        }

        public void SetData<T>(string name, T data)
        {
            _data[name] = JValue.FromObject(data);
        }

        [JsonConstructor]
        internal EnrichedActivity()
        {
        }

        internal static EnrichedActivity FromJson(string json)
        {
            return FromJson(JObject.Parse(json));
        }

        internal static EnrichedActivity FromJson(JObject obj)
        {
            EnrichedActivity activity = new EnrichedActivity();
            EnrichedAggregatedActivity aggregateActivity = null;
            EnrichedNotificationActivity notificationActivity = null;

            if (obj.Properties().Any(p => p.Name == Field_Activities))
            {
                if ((obj.Properties().Any(p => p.Name == Field_IsRead)) ||
                    (obj.Properties().Any(p => p.Name == Field_IsSeen)))
                {
                    activity = aggregateActivity = notificationActivity = new EnrichedNotificationActivity();
                }
                else
                {
                    activity = aggregateActivity = new EnrichedAggregatedActivity();
                }
            }

            obj.Properties().ForEach((prop) =>
            {
                switch (prop.Name)
                {
                    case Field_Id: activity.Id = prop.Value.Value<string>(); break;
                    case Field_Actor: activity.Actor = EnrichableField.FromJSON(prop.Value); break;
                    case Field_Verb: activity.Verb = EnrichableField.FromJSON(prop.Value); break;
                    case Field_Object: activity.Object = EnrichableField.FromJSON(prop.Value); break;
                    case Field_Target: activity.Target = EnrichableField.FromJSON(prop.Value); break;
                    case Field_ForeignId: activity.ForeignId = EnrichableField.FromJSON(prop.Value); break;
                    case Field_Time: activity.Time = prop.Value.Value<DateTime>(); break;
                    case Field_To:
                        {
                            JArray array = prop.Value as JArray;
                            if ((array != null) && (array.SafeCount() > 0))
                            {
                                if (array.First.Type == JTokenType.Array)
                                {
                                    // need to take the first from each array
                                    List<string> tos = new List<string>();

                                    foreach (var child in array)
                                    {
                                        var str = child.ToObject<string[]>();
                                        tos.Add(str[0]);
                                    }

                                    activity.To = tos;
                                }
                                else
                                {
                                    activity.To = prop.Value.ToObject<string[]>().ToList();
                                }
                            }
                            else
                            {
                                activity.To = new List<string>();
                            }
                            break;
                        }
                    case Field_Activities:
                        {
                            var activities = new List<EnrichedActivity>();

                            JArray array = prop.Value as JArray;
                            if ((array != null) && (array.SafeCount() > 0))
                            {
                                foreach (var child in array)
                                {
                                    var childJO = child as JObject;
                                    if (childJO != null)
                                        activities.Add(FromJson(childJO));
                                }
                            }

                            if (aggregateActivity != null)
                                aggregateActivity.Activities = activities;
                            break;
                        }
                    case Field_ActorCount:
                        {
                            if (aggregateActivity != null)
                                aggregateActivity.ActorCount = prop.Value.Value<int>();
                            break;
                        }
                    case Field_IsRead:
                        {
                            if (notificationActivity != null)
                                notificationActivity.IsRead = prop.Value.Value<bool>();
                            break;
                        }
                    case Field_IsSeen:
                        {
                            if (notificationActivity != null)
                                notificationActivity.IsSeen = prop.Value.Value<bool>();
                            break;
                        }
                    case Field_CreatedAt:
                        {
                            if (aggregateActivity != null)
                                aggregateActivity.CreatedAt = prop.Value.Value<DateTime>();
                            break;
                        }
                    case Field_UpdatedAt:
                        {
                            if (aggregateActivity != null)
                                aggregateActivity.UpdatedAt = prop.Value.Value<DateTime>();
                            break;
                        }
                    case Field_Group:
                        {
                            if (aggregateActivity != null)
                                aggregateActivity.Group = prop.Value.Value<string>();
                            break;
                        }
                    case Field_OwnReactions:
                        {
                            activity.OwnReactions = prop.Value.ToObject<IDictionary<string, IEnumerable<Reaction>>>();
                            break;
                        }
                    case Field_LatestReactions:
                        {
                            activity.LatestReactions = prop.Value.ToObject<IDictionary<string, IEnumerable<Reaction>>>();
                            break;
                        }
                    case Field_ReactionCounts:
                        {
                            activity.ReactionCounts = prop.Value.ToObject<IDictionary<string, int>>();
                            break;
                        }
                    default:
                        {
                            // stash everything else as custom
                            activity._data[prop.Name] = prop.Value;
                            break;
                        };
                }
            });
            return activity;
        }
    }
}
