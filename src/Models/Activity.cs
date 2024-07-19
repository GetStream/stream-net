using System;
using System.Collections.Generic;

namespace Stream.Models
{
    public abstract class ActivityBase : CustomDataBase
    {
        public string Id { get; set; }
        public string Verb { get; set; }
        public string ForeignId { get; set; }
        public DateTime? Time { get; set; }
        public List<string> To { get; set; }
        public float? Score { get; set; }

        public string Ref() => $"SA:{Id}";
    }

    public class Activity : ActivityBase
    {
        public string Actor { get; set; }
        public string Object { get; set; }
        public string Target { get; set; }
        public string Origin { get; set; }
        public Dictionary<string, object> ScoreVars { get; set; }

        public string ModerationTemplate { get; set; }
        public ModerationContent Moderation { get; }

        public Activity(string actor, string verb, string @object)
        {
            Actor = actor;
            Verb = verb;
            Object = @object;
        }
    }

    public class AddActivityResponse : ResponseBase
    {
        public Activity Activity { get; set; }
    }

    public class AddActivitiesResponse : ResponseBase
    {
        public List<Activity> Activities { get; set; }
    }

    public class ActivityPartialUpdateRequestObject
    {
        public string Id { get; set; }
        public string ForeignId { get; set; }
        public Dictionary<string, object> Set { get; set; }
        public IEnumerable<string> Unset { get; set; }
        public DateTime? Time { get; set; }
    }
    
    public class ModerationContent {
        public string ConfigKey { get; set; }
        public List<string> Texts { get; set; }
        public List<string> Images { get; set; }
    }
}
