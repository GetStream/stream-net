using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GetStream
{
    public class UpdateToTargetsResponse
    {
        public Activity Activity { get; internal set; }
        public List<string> Added { get; internal set; }
        public List<string> Removed { get; internal set; }

        public UpdateToTargetsResponse() { }

        internal static UpdateToTargetsResponse FromJson(string json)
        {
            return UpdateToTargetsResponse.FromJson(JObject.Parse(json));
        }

        internal static UpdateToTargetsResponse FromJson(JObject obj)
        {
            var result = new UpdateToTargetsResponse();
            foreach (var jsonProp in obj.Properties())
            {
                switch (jsonProp.Name)
                {
                    case "added":
                        result.Added = jsonProp.Value.ToObject<List<string>>();
                        break;
                    case "removed":
                        result.Removed = jsonProp.Value.ToObject<List<string>>();
                        break;
                    case "activity":
                        result.Activity = Activity.FromJson(jsonProp.Value.ToObject<JObject>());
                        break;
                }
            }
            return result;
        }
    }
}
