using FluentAssertions;
using GraphBridge.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;

namespace GraphBridge.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock = new();

    /// <summary>
    /// Custom response feature that captures OnStarting callbacks and fires them on demand,
    /// allowing middleware that registers OnStarting delegates to be tested.
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

    [Fact]
    public async Task InvokeAsync_GeneratesValidGuid_StoredInHttpContextItems()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().NotBeNull();
        var correlationId = context.Items["CorrelationId"]!.ToString();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_GeneratesUniqueGuidPerRequest()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            _loggerMock.Object);

        var correlationIds = new HashSet<string>();

        // Act — invoke multiple requests
        for (int i = 0; i < 100; i++)
        {
            var context = new DefaultHttpContext();
            await middleware.InvokeAsync(context);
            var id = context.Items["CorrelationId"]!.ToString()!;
            correlationIds.Add(id);
        }

        // Assert — all 100 IDs should be unique
        correlationIds.Should().HaveCount(100);
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToResponseHeader()
    {
        // Arrange
        var (context, responseFeature) = CreateContextWithTestFeature();

        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);
        await responseFeature.FireOnStartingAsync();

        // Assert
        responseFeature.Headers["X-Correlation-ID"].ToString().Should().NotBeNullOrEmpty();
        Guid.TryParse(responseFeature.Headers["X-Correlation-ID"].ToString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToLoggingScope()
    {
        // Arrange
        var context = new DefaultHttpContext();
        string? capturedCorrelationId = null;

        _loggerMock
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback<object>(state =>
            {
                if (state is Dictionary<string, object> dict && dict.ContainsKey("CorrelationId"))
                {
                    capturedCorrelationId = dict["CorrelationId"]?.ToString();
                }
            })
            .Returns(Mock.Of<IDisposable>());

        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        capturedCorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(capturedCorrelationId, out _).Should().BeTrue();

        _loggerMock.Verify(
            l => l.BeginScope(It.IsAny<It.IsAnyType>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdInResponseMatchesContextItem()
    {
        // Arrange
        var (context, responseFeature) = CreateContextWithTestFeature();

        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);
        await responseFeature.FireOnStartingAsync();

        // Assert
        var itemId = context.Items["CorrelationId"]!.ToString();
        var headerId = responseFeature.Headers["X-Correlation-ID"].ToString();
        headerId.Should().Be(itemId);
    }
}
