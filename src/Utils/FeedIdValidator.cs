using System;

namespace Stream.Utils
{
    public static class FeedIdValidator
    {
        /// <summary>
        /// Validates a fully qualified feed identifier. It should look like this: flat:myfeedname
        /// We could use a Regex but that has a performance impact
        /// so let's just iterate through the string and check for the correct format.
        /// </summary>
        public static void ThrowIfFeedIdIsInvalid(string feedId)
        {
            if (string.IsNullOrWhiteSpace(feedId))
            {
                throw new InvalidFeedIdException(feedId);
            }

            var foundColon = false;
            var colonIndex = 0;
            var index = 0;

            foreach (var character in feedId)
            {
                if (character == ':')
                {
                    if (foundColon)
                    {
                        throw new InvalidFeedIdException(feedId);
                    }

                    if (index == 0 || index == feedId.Length - 1)
                    {
                        throw new InvalidFeedIdException(feedId);
                    }

                    foundColon = true;
                    colonIndex = index;
                }

                index++;
            }

            if (!foundColon)
            {
                throw new InvalidFeedIdException(feedId);
            }
        }
    }

    /// <summary>
    /// Exception thrown when a feed identifier is invalid.
    /// The feed identifier should have a single colon. Example: flat:myfeedname
    /// </summary>
    public class InvalidFeedIdException : Exception
    {
        public InvalidFeedIdException(string feedId) : base($"Invalid feed id: {feedId}. It should look like this: flat:myfeedname")
        {
        }
    }
}