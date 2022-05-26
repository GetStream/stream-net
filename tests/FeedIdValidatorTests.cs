using NUnit.Framework;
using Stream.Utils;

public class FeedIdValidatorTests
{
    [Test]
    public void TestFeedIdValidator()
    {
        var invalidFeedIds = new[] { "nocolon", ":beginning", "ending:", "mul:tip:le:colons" };
        var valifFeedIds = new[] { "flat:myfeedname" };

        foreach (var feedId in invalidFeedIds)
        {
            Assert.Throws<InvalidFeedIdException>(() => FeedIdValidator.ThrowIfFeedIdIsInvalid(feedId));
        }

        foreach (var feedId in valifFeedIds)
        {
            Assert.DoesNotThrow(() => FeedIdValidator.ThrowIfFeedIdIsInvalid(feedId));
        }
    }
}