using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class AggregateActivity : Activity
    {
        public IList<Activity> Activities { get; internal set; }

        public int ActorCount { get; internal set; }

        [JsonConstructor]
        internal AggregateActivity()
        {
        }
    }
}
