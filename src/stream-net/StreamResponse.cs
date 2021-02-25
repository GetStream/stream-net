using System.Collections.Generic;

namespace Stream
{
    public class StreamResponse<T>
    {
        public string Duration
        {
            get;
            internal set;
        }

        public IEnumerable<T> Results
        {
            get;
            internal set;
        }

        public long Unseen
        {
            get;
            internal set;
        }

        public long Unread
        {
            get;
            internal set;
        }
    }
}
