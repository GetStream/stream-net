using System.Collections.Generic;

namespace Stream.Models
{
    /// <summary>Base class for read responses of <typeparamref name="T"/>.</summary>
    public class GenericGetResponse<T> : ResponseBase
    {
        /// <summary>Container for <typeparamref name="T"/> objects.</summary>
        public List<T> Results { get; set; }
    }

    /// <summary>Base class for personalized read responses of <typeparamref name="T"/>.</summary>
    public class PersonalizedGetResponse<T> : GenericGetResponse<T>
    {
        public int Limit { get; set; }
        public string Next { get; set; }
        public int Offset { get; set; }
        public string Version { get; set; }
    }
}
