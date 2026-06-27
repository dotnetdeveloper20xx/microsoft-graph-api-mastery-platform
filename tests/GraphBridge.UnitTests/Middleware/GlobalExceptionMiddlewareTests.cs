using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using GraphBridge.Api.Middleware;
using GraphBridge.Shared;
using GraphBridge.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using AuthenticationException = GraphBridge.Shared.Exceptions.AuthenticationException;

namespace GraphBridge.UnitTests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private GlobalExceptionMiddleware CreateMiddleware(Func<HttpContext, Task> next)
    {
        return new GlobalExceptionMiddleware(
            next: new RequestDelegate(next),
            _loggerMock.Object);
    }

    private async Task<ApiEnvelope<object>?> GetEnvelopeFromResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiEnvelope<object>>(json, JsonOptions);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CorrelationId"] = Guid.NewGuid().ToString();
        return context;
    }

    #region ValidationException → 400

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400WithValidationFailedMessage()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email must be a valid email address")
        };
        var validationException = new ValidationException(failures);

        var middleware = CreateMiddleware(_ => throw validationException);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Be("Validation failed");
        envelope.Errors.Should().HaveCount(2);
        envelope.Errors[0].Field.Should().Be("Name");
        envelope.Errors[0].Detail.Should().Be("Name is required");
        envelope.Errors[1].Field.Should().Be("Email");
        envelope.Errors[1].Detail.Should().Be("Email must be a valid email address");
    }

    #endregion

    #region NotFoundException → 404

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404WithResourceMessage()
    {
        // Arrange
        var notFoundEx = new NotFoundException("Employee", Guid.NewGuid());
        var middleware = CreateMiddleware(_ => throw notFoundEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(404);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Contain("Employee");
        envelope.Errors.Should().BeEmpty();
    }

    #endregion

    #region BusinessRuleException → 422

    [Fact]
    public async Task InvokeAsync_BusinessRuleException_Returns422WithRuleMessage()
    {
        // Arrange
        var businessRuleEx = new BusinessRuleException("Communication pack must be generated before sending emails");
        var middleware = CreateMiddleware(_ => throw businessRuleEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(422);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Be("Communication pack must be generated before sending emails");
        envelope.Errors.Should().BeEmpty();
    }

    #endregion

    #region GraphServiceException → 502

    [Fact]
    public async Task InvokeAsync_GraphServiceException_Returns502WithGenericMessage()
    {
        // Arrange
        var graphEx = new GraphServiceException("GetUserProfile", "Network timeout");
        var middleware = CreateMiddleware(_ => throw graphEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(502);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Be("A downstream service error occurred");
        envelope.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_GraphServiceException_DoesNotLeakOperationDetails()
    {
        // Arrange
        var graphEx = new GraphServiceException("GetUserProfile", "Secret internal auth token expired for tenant abc123");
        var middleware = CreateMiddleware(_ => throw graphEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var envelope = await GetEnvelopeFromResponse(context);
        envelope!.Message.Should().NotContain("GetUserProfile");
        envelope.Message.Should().NotContain("Secret internal auth token");
        envelope.Message.Should().NotContain("abc123");
    }

    #endregion

    #region AuthenticationException → 401

    [Fact]
    public async Task InvokeAsync_AuthenticationException_Returns401WithAuthMessage()
    {
        // Arrange
        var authEx = new AuthenticationException("Token acquisition failed due to invalid credentials");
        var middleware = CreateMiddleware(_ => throw authEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Be("Token acquisition failed due to invalid credentials");
        envelope.Errors.Should().BeEmpty();
    }

    #endregion

    #region Unknown Exception → 500

    [Fact]
    public async Task InvokeAsync_UnknownException_Returns500WithGenericMessage()
    {
        // Arrange
        var unknownEx = new InvalidOperationException("Something broke internally at line 42 of SecretService.cs");
        var middleware = CreateMiddleware(_ => throw unknownEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Be("An unexpected error occurred. Please reference the correlationId for support.");
        envelope.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_NullReferenceException_Returns500()
    {
        // Arrange
        var ex = new NullReferenceException("Object reference not set to an instance of an object");
        var middleware = CreateMiddleware(_ => throw ex);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);

        var envelope = await GetEnvelopeFromResponse(context);
        envelope!.Success.Should().BeFalse();
        envelope.Message.Should().Be("An unexpected error occurred. Please reference the correlationId for support.");
    }

    #endregion

    #region No Stack Traces or Internal Details Leak

    [Fact]
    public async Task InvokeAsync_UnknownException_DoesNotLeakStackTrace()
    {
        // Arrange
        Exception thrownEx;
        try
        {
            throw new InvalidOperationException("Internal database connection string: Server=prod-db;Password=secret123");
        }
        catch (Exception ex)
        {
            thrownEx = ex; // This exception now has a real stack trace
        }

        var middleware = CreateMiddleware(_ => throw thrownEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var rawJson = await reader.ReadToEndAsync();

        rawJson.Should().NotContain("StackTrace");
        rawJson.Should().NotContain("at GraphBridge");
        rawJson.Should().NotContain("at System");
        rawJson.Should().NotContain("prod-db");
        rawJson.Should().NotContain("Password=secret123");
        rawJson.Should().NotContain("Internal database connection string");
    }

    [Fact]
    public async Task InvokeAsync_GraphServiceException_DoesNotLeakStackTrace()
    {
        // Arrange
        Exception thrownEx;
        try
        {
            throw new GraphServiceException("GetUsers", "Tenant secret: xyz789-internal-key");
        }
        catch (Exception ex)
        {
            thrownEx = ex;
        }

        var middleware = CreateMiddleware(_ => throw thrownEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var rawJson = await reader.ReadToEndAsync();

        rawJson.Should().NotContain("StackTrace");
        rawJson.Should().NotContain("at GraphBridge");
        rawJson.Should().NotContain("xyz789-internal-key");
        rawJson.Should().NotContain("GetUsers");
    }

    [Fact]
    public async Task InvokeAsync_AuthenticationException_DoesNotLeakInnerExceptionDetails()
    {
        // Arrange
        var innerEx = new Exception("MSAL token cache corrupted at path C:\\secrets\\token-cache.dat");
        var authEx = new AuthenticationException("Authentication failed", innerEx);
        var middleware = CreateMiddleware(_ => throw authEx);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var rawJson = await reader.ReadToEndAsync();

        rawJson.Should().NotContain("MSAL token cache corrupted");
        rawJson.Should().NotContain("C:\\secrets\\token-cache.dat");
        rawJson.Should().NotContain("InnerException");
    }

    #endregion

    #region CorrelationId Preserved in Envelope

    [Fact]
    public async Task InvokeAsync_AnyException_PreservesCorrelationIdInEnvelope()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var context = CreateHttpContext();
        context.Items["CorrelationId"] = correlationId;

        var middleware = CreateMiddleware(_ => throw new Exception("fail"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var envelope = await GetEnvelopeFromResponse(context);
        envelope!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_MissingCorrelationId_GeneratesNewGuidInEnvelope()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        // No CorrelationId set in Items

        var middleware = CreateMiddleware(_ => throw new Exception("fail"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var envelope = await GetEnvelopeFromResponse(context);
        envelope!.CorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(envelope.CorrelationId, out _).Should().BeTrue();
    }

    #endregion

    #region Response Content-Type

    [Fact]
    public async Task InvokeAsync_AnyException_SetsContentTypeToApplicationJson()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new Exception("fail"));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    #endregion

    #region API Envelope Format

    [Fact]
    public async Task InvokeAsync_AnyException_ReturnsValidApiEnvelopeWithTimestamp()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Order", 42));
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var envelope = await GetEnvelopeFromResponse(context);
        envelope.Should().NotBeNull();
        envelope!.Timestamp.Should().NotBeNullOrEmpty();

        // Verify ISO 8601 format
        DateTimeOffset.TryParse(envelope.Timestamp, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_NoException_DoesNotIntercept()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200); // Default status code unchanged
    }

    #endregion
}
