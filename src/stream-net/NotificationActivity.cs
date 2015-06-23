using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class NotificationActivity : AggregateActivity
    {
        public bool IsRead { get; set; }

        public bool IsSeen { get; set; }

        [JsonConstructor]
        internal NotificationActivity()
        {
        }
    }
}
