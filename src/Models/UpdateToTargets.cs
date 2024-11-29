using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stream.Models
{
    public class UpdateToTargetsRequest
    {
        [JsonProperty("foreign_id")]
        public string ForeignID { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("new_targets")]
        public List<string> NewTargets { get; set; }

        [JsonProperty("added_targets")]
        public List<string> Adds { get; set; }

        [JsonProperty("removed_targets")]
        public List<string> RemovedTargets { get; set; }
    }
}
