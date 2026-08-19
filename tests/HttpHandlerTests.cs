using NUnit.Framework;
using Stream;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace StreamNetTests
{
    [TestFixture]
    public class HttpHandlerTests
    {
        private const long ResetTimestamp = 1717171717;

        [Test]
        public async Task RateLimitHandlerReadsRateLimitHeaders()
        {
            var rateLimits = new RateLimitHandler();
            var client = ClientWith(rateLimits, StubHandler.Ok());

            await client.Feed("flat", "userid").GetActivitiesAsync();

            Assert.AreEqual(5000, rateLimits.Current.Limit);
            Assert.AreEqual(4999, rateLimits.Current.Remaining);
            Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(ResetTimestamp), rateLimits.Current.Reset);
        }

        [Test]
        public async Task RateLimitHandlerInvokesCallbackPerResponse()
        {
            var observed = new List<StreamRateLimits>();
            var client = ClientWith(new RateLimitHandler(observed.Add), StubHandler.Ok());
            var feed = client.Feed("flat", "userid");

            await feed.GetActivitiesAsync();
            await feed.GetActivitiesAsync();

            Assert.AreEqual(2, observed.Count);
            Assert.AreEqual(4999, observed[0].Remaining);
            Assert.AreEqual(4999, observed[1].Remaining);
        }

        [Test]
        public async Task RateLimitHandlerReportsNothingWhenHeadersAreAbsent()
        {
            var rateLimits = new RateLimitHandler();
            var client = ClientWith(rateLimits, StubHandler.WithoutRateLimitHeaders());

            await client.Feed("flat", "userid").GetActivitiesAsync();

            Assert.IsNull(rateLimits.Current);
        }

        [Test]
        public async Task HandlersSeeTheOutgoingRequest()
        {
            var stub = StubHandler.Ok();
            var client = ClientWith(stub);

            await client.Feed("flat", "userid").GetActivitiesAsync();

            Assert.AreEqual(HttpMethod.Get, stub.LastRequest.Method);
            StringAssert.Contains("/feed/flat/userid/", stub.LastRequest.RequestUri.AbsolutePath);
            Assert.IsTrue(stub.LastRequest.Headers.Contains("Authorization"));
        }

        [Test]
        public async Task HandlersRunOutermostFirst()
        {
            var calls = new List<string>();
            var client = ClientWith(new RecordingHandler("outer", calls), new RecordingHandler("inner", calls), StubHandler.Ok());

            await client.Feed("flat", "userid").GetActivitiesAsync();

            Assert.AreEqual(new[] { "outer:request", "inner:request", "inner:response", "outer:response" }, calls);
        }

        [Test]
        public void NoHandlersAreRegisteredByDefault()
        {
            Assert.IsEmpty(StreamClientOptions.Default.HttpHandlers);
        }

        private static IStreamClient ClientWith(params DelegatingHandler[] handlers)
        {
            var options = new StreamClientOptions();
            foreach (var handler in handlers)
            {
                options.HttpHandlers.Add(handler);
            }

            return new StreamClient("apikey", "apisecret", options);
        }

        // Terminal handler: answers with a canned response so no request leaves the machine.
        private class StubHandler : DelegatingHandler
        {
            private readonly bool _withRateLimitHeaders;

            private StubHandler(bool withRateLimitHeaders)
            {
                _withRateLimitHeaders = withRateLimitHeaders;
            }

            public HttpRequestMessage LastRequest { get; private set; }

            public static StubHandler Ok() => new StubHandler(true);

            public static StubHandler WithoutRateLimitHeaders() => new StubHandler(false);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"results\":[]}"),
                };

                if (_withRateLimitHeaders)
                {
                    response.Headers.Add("X-RateLimit-Limit", "5000");
                    response.Headers.Add("X-RateLimit-Remaining", "4999");
                    response.Headers.Add("X-RateLimit-Reset", ResetTimestamp.ToString());
                }

                return Task.FromResult(response);
            }
        }

        private class RecordingHandler : DelegatingHandler
        {
            private readonly string _name;
            private readonly List<string> _calls;

            public RecordingHandler(string name, List<string> calls)
            {
                _name = name;
                _calls = calls;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _calls.Add($"{_name}:request");
                var response = await base.SendAsync(request, cancellationToken);
                _calls.Add($"{_name}:response");
                return response;
            }
        }
    }

    // Rate limit budget reported by the Stream API on a single response.
    // See https://getstream.io/docs/platform/rate-limits/ for how Stream applies rate limits.
    internal class StreamRateLimits
    {
        public StreamRateLimits(int limit, int remaining, DateTimeOffset reset)
        {
            Limit = limit;
            Remaining = remaining;
            Reset = reset;
        }

        public int Limit { get; }

        public int Remaining { get; }

        public DateTimeOffset Reset { get; }
    }

    // Sample handler proving StreamClientOptions.HttpHandlers can reach response headers the SDK
    // does not surface itself. This is the handler the README documents users writing themselves,
    // kept here so the extension point stays covered without shipping it as public API.
    internal class RateLimitHandler : DelegatingHandler
    {
        private readonly Action<StreamRateLimits> _onResponse;
        private volatile StreamRateLimits _current;

        public RateLimitHandler(Action<StreamRateLimits> onResponse = null)
        {
            _onResponse = onResponse;
        }

        // Budget reported by the most recent response, or null until one carried the headers.
        public StreamRateLimits Current => _current;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (TryReadRateLimits(response.Headers, out var limits))
            {
                _current = limits;
                _onResponse?.Invoke(limits);
            }

            return response;
        }

        private static bool TryReadRateLimits(HttpResponseHeaders headers, out StreamRateLimits limits)
        {
            limits = null;

            if (!TryReadNumber(headers, "X-RateLimit-Limit", out var limit) ||
                !TryReadNumber(headers, "X-RateLimit-Remaining", out var remaining) ||
                !TryReadNumber(headers, "X-RateLimit-Reset", out var reset))
            {
                return false;
            }

            limits = new StreamRateLimits((int)limit, (int)remaining, DateTimeOffset.FromUnixTimeSeconds(reset));
            return true;
        }

        private static bool TryReadNumber(HttpResponseHeaders headers, string name, out long value)
        {
            value = 0;

            if (!headers.TryGetValues(name, out var values))
                return false;

            foreach (var candidate in values)
            {
                if (long.TryParse(candidate, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    return true;
            }

            return false;
        }
    }
}
