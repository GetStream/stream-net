using System.Collections.Generic;

namespace Stream.Models
{
    public class UpdateToTargetsResponse
    {
        public Activity Activity { get; set; }
        public List<string> Added { get; set; }
        public List<string> Removed { get; set; }
    }
}
