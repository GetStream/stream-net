using System.Collections.Generic;

namespace Stream.Models
{
    /// <summary>Base class for read responses of <typeparamref name="T"/>.</summary>
    public class GenericGetResponse<T> : ResponseBase
    {
        /// <summary>Container for <typeparamref name="T"/> objects.</summary>
        public List<T> Results { get; set; }
    }
}