using System;

namespace Stream.Models
{
    public class ForeignIdTime
    {
        public string ForeignId { get; set; }
        public DateTime Time { get; set; }

        public ForeignIdTime(string foreignId, DateTime time)
        {
            ForeignId = foreignId;
            Time = time;
        }
    }
}
