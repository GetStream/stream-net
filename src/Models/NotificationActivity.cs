using System.Collections.Generic;

namespace Stream.Models
{
    public abstract class NotificationActivityBase : AggregateActivityBase
    {
        /// <summary>Whether the activity was read.</summary>
        public bool IsRead { get; set; }

        /// <summary>Whether he activity was seen.</summary>
        public bool IsSeen { get; set; }
    }

    public class NotificationActivity : NotificationActivityBase
    {
        public List<Activity> Activities { get; set; }
    }

    public class EnrichedNotificationActivity : NotificationActivityBase
    {
        public List<EnrichedActivity> Activities { get; set; }
    }

    public class NotificationGetResponse : GenericGetResponse<NotificationActivity>
    {
        public int Unseen { get; set; }
        public int Unread { get; set; }
    }
}
