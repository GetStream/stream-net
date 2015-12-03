using Newtonsoft.Json;

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
