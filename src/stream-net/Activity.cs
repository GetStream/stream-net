using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class Activity
    {
        private const string Field_Id = "id";
        private const string Field_Actor = "actor";
        private const string Field_Verb = "verb";
        private const string Field_Object = "object";
        private const string Field_ForeignId = "foreign_id";
        private const string Field_To = "to";
        private const string Field_Target = "target";
        private const string Field_Time = "time";

        readonly IDictionary<string, string> _data = new Dictionary<string, string>();

        public String Id { get; set; }

        public String Actor { get; set; }

        public String Verb { get; set; }

        public String Object { get; set; }

        public String Target { get; set; }

        public DateTime? Time { get; set; }

        [JsonProperty("foreign_id")]
        public String ForeignId { get; set; }

        public IList<String> To { get; set; }

        public T GetData<T>(String name)
        {
            if (_data.ContainsKey(name))
            {
                return JsonConvert.DeserializeObject<T>(_data[name]);
            }
            return default(T);
        }

        public void SetData<T>(String name, T data)
        {
            _data[name] = JsonConvert.SerializeObject(data);
        }

        [JsonConstructor]
        internal Activity()
        {

        }

        public Activity(String actor, String verb, String @object)
        {
            Actor = actor;
            Verb = verb;
            Object = @object;
        }

        internal String ToJson(StreamClient client)
        {
            JObject obj = new JObject(
                new JProperty(Field_Actor, this.Actor),
                new JProperty(Field_Verb, this.Verb),
                new JProperty(Field_Object, this.Object));

            if (Time.HasValue)            
                obj.Add(new JProperty(Field_Time, this.Time.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture)));

            if (!String.IsNullOrWhiteSpace(ForeignId))
                obj.Add(new JProperty(Field_ForeignId, this.ForeignId));

            if (!String.IsNullOrWhiteSpace(Target))
                obj.Add(new JProperty(Field_Target, this.Target));            

            if (To.SafeCount() > 0)
            {
                JArray toArray = new JArray();
                (from t in To select client.SignTo(t)).ForEach((st) =>
                {
                    toArray.Add(st);
                });
                obj.Add(new JProperty(Field_To, toArray));
            }

            if (_data.SafeCount() > 0)
            {
                _data.Keys.ForEach((k) =>
                {
                    var t = JToken.Parse(_data[k]);
                    obj.Add(new JProperty(k, t));
                });
            }

            return obj.ToString();
        }

        internal static Activity FromJson(String json)
        {
            return FromJson(JObject.Parse(json));
        }

        internal static Activity FromJson(JObject obj)
        {
            Activity activity = new Activity();
            obj.Properties().ForEach((prop) =>
            {
                switch (prop.Name)
                {
                    case Field_Id: activity.Id = prop.Value.Value<String>(); break;
                    case Field_Actor: activity.Actor = prop.Value.Value<String>(); break;
                    case Field_Verb: activity.Verb = prop.Value.Value<String>(); break;
                    case Field_Object: activity.Object = prop.Value.Value<String>(); break;
                    case Field_Target: activity.Target = prop.Value.Value<String>(); break;
                    case Field_ForeignId: activity.ForeignId = prop.Value.Value<String>(); break;
                    case Field_Time: activity.Time = prop.Value.Value<DateTime>(); break;
                    case Field_To:
                    {
                        List<String> tos = new List<string>();
                        //foreach (var ) {

                        //}
                        activity.To = tos;
                        break;
                    }
                    default:
                    {
                        // stash everything else as custom
                        activity._data[prop.Name] = prop.Value.ToString();
                        break;
                    };
                }
            });
            return activity;
        }      
    }
}
