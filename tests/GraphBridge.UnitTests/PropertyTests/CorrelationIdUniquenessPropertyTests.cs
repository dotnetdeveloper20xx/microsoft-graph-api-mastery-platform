using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property 2: CorrelationId Uniqueness
/// For any sequence of HTTP requests to the platform, each response SHALL contain a unique,
/// valid GUID correlationId that differs from all other correlationIds in the sequence,
/// and the same correlationId SHALL appear in all structured log entries produced during
/// that request's processing.
/// 
/// **Validates: Requirements 2.5**
/// </summary>
public class CorrelationIdUniquenessPropertyTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock = new();

    /// <summary>
    /// Custom response feature that captures OnStarting callbacks and fires them on demand.
    /// </summary>
    private class TestHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStartingCallbacks = new();

        public int StatusCode { get; set; } = 200;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            _onStartingCallbacks.Add((callback, state));
        }

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public async Task FireOnStartingAsync()
        {
            HasStarted = true;
            foreach (var (callback, state) in _onStartingCallbacks)
            {
                await callback(state);
            }
        }
    }

    private (HttpContext Context, TestHttpResponseFeature ResponseFeature) CreateContextWithTestFeature()
    {
        var responseFeature = new TestHttpResponseFeature();
        var features = new FeatureCollection();
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpRequestFeature>(new HttpRequestFeature());

        var context = new DefaultHttpContext(features);
        return (context, responseFeature);
    }

    /// <summary>
    /// For any sequence of N requests (2-50), all correlationIds stored in HttpContext.Items
    /// are unique and each is a valid GUID.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AllCorrelationIds_AreUnique_AndValidGuids()
    {
        return Prop.ForAll(
            Gen.Choose(2, 50).ToArbitrary(),
            requestCount =>
            {
                var middleware = new CorrelationIdMiddleware(
                    next: _ => Task.CompletedTask,
                    _loggerMock.Object);

                var correlationIds = new List<string>();

                for (int i = 0; i < requestCount; i++)
                {
                    var context = new DefaultHttpContext();
                    middleware.InvokeAsync(context).GetAwaiter().GetResult();

                    var correlationId = context.Items["CorrelationId"]?.ToString();

                    // Each correlationId must be non-null/non-empty
                    correlationId.Should().NotBeNullOrEmpty(
                        $"Request {i} should have a non-null correlationId");

                    // Each correlationId must be a valid GUID
                    Guid.TryParse(correlationId, out _).Should().BeTrue(
                        $"CorrelationId '{correlationId}' should be a valid GUID");

                    correlationIds.Add(correlationId!);
                }

                // All correlationIds must be unique
                correlationIds.Distinct().Count().Should().Be(correlationIds.Count,
                    "all correlationIds in a sequence of requests must be unique");
            });
    }

    /// <summary>
    /// For any sequence of N requests (2-50), the correlationId in the response header
    /// X-Correlation-ID matches the correlationId stored in HttpContext.Items for each request.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CorrelationId_InResponseHeader_MatchesHttpContextItems()
    {
        return Prop.ForAll(
            Gen.Choose(2, 50).ToArbitrary(),
            requestCount =>
            {
                var middleware = new CorrelationIdMiddleware(
                    next: _ => Task.CompletedTask,
                    _loggerMock.Object);

                for (int i = 0; i < requestCount; i++)
                {
                    var (context, responseFeature) = CreateContextWithTestFeature();
                    middleware.InvokeAsync(context).GetAwaiter().GetResult();
                    responseFeature.FireOnStartingAsync().GetAwaiter().GetResult();

                    var itemId = context.Items["CorrelationId"]?.ToString();
                    var headerId = responseFeature.Headers["X-Correlation-ID"].ToString();

                    // Header must match context item
                    headerId.Should().Be(itemId,
                        $"Request {i}: response header correlationId must match HttpContext.Items value");
                }
            });
    }

    /// <summary>
    /// For any sequence of N requests (2-50), no two consecutive requests share the same correlationId.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NoTwoConsecutiveRequests_ShareSameCorrelationId()
    {
        return Prop.ForAll(
            Gen.Choose(2, 50).ToArbitrary(),
            requestCount =>
            {
                var middleware = new CorrelationIdMiddleware(
                    next: _ => Task.CompletedTask,
                    _loggerMock.Object);

                string? previousCorrelationId = null;

                for (int i = 0; i < requestCount; i++)
                {
                    var context = new DefaultHttpContext();
                    middleware.InvokeAsync(context).GetAwaiter().GetResult();

                    var currentCorrelationId = context.Items["CorrelationId"]?.ToString();

                    if (previousCorrelationId != null)
                    {
                        currentCorrelationId.Should().NotBe(previousCorrelationId,
                            $"Request {i} should not share correlationId with request {i - 1}");
                    }

                    previousCorrelationId = currentCorrelationId;
                }
            });
    }

    /// <summary>
    /// For any sequence of N requests (2-50), each correlationId is a parseable GUID
    /// (verifying format compliance beyond simple non-null checks).
    /// </summary>
    [Property(MaxTest = 100)]
    public Property EachCorrelationId_IsParseableAsGuid()
    {
        return Prop.ForAll(
            Gen.Choose(2, 50).ToArbitrary(),
            requestCount =>
            {
                var middleware = new CorrelationIdMiddleware(
                    next: _ => Task.CompletedTask,
                    _loggerMock.Object);

                for (int i = 0; i < requestCount; i++)
                {
                    var context = new DefaultHttpContext();
                    middleware.InvokeAsync(context).GetAwaiter().GetResult();

                    var correlationId = context.Items["CorrelationId"]?.ToString();

                    // Must be parseable as a Guid
                    var isValidGuid = Guid.TryParse(correlationId, out var parsed);
                    isValidGuid.Should().BeTrue(
                        $"CorrelationId '{correlationId}' at request {i} must be a valid GUID");

                    // Parsed GUID should not be empty
                    parsed.Should().NotBe(Guid.Empty,
                        $"CorrelationId at request {i} should not be Guid.Empty");
                }
            });
    }
}
