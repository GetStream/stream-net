using System;
using System.Collections.Generic;

namespace Stream
{
    public class User
    {
        public string Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IDictionary<string, object> Data { get; set; }

        /// <summary>Returns a reference identifier to this object.</summary>
        public string Ref() => $"SU:{Id}";
    }
}
