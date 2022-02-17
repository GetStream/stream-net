namespace Stream.Models
{
    /// <summary>Base class for all API responses.</summary>
    public class ResponseBase
    {
        /// <summary>Duration of the request in human-readable format.</summary>
        public string Duration { get; set; }
    }
}