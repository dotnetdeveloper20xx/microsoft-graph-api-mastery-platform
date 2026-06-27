using System.Text.Json;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Api.Middleware;
using GraphBridge.Shared;
using GraphBridge.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using AuthenticationException = GraphBridge.Shared.Exceptions.AuthenticationException;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property 4: Exception-to-Status Mapping
/// For any unhandled exception thrown during request processing, the global exception middleware
/// SHALL map it to the correct HTTP status code (ValidationException→400, NotFoundException→404,
/// BusinessRuleException→422, GraphServiceException→502, AuthenticationException→401, all others→500)
/// and return an API_Envelope with success=false, no internal details or stack traces exposed,
/// and the correlationId preserved in both the response body and log output.
///
/// **Validates: Requirements 2.4, 2.6, 4.6, 12.5**
/// </summary>
public class ExceptionToStatusMappingPropertyTests
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

    private static HttpContext CreateHttpContext(string? correlationId = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items["CorrelationId"] = correlationId ?? Guid.NewGuid().ToString();
        return context;
    }

    private static async Task<(ApiEnvelope<object>? Envelope, string RawJson)> GetResponseFromContext(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<object>>(json, JsonOptions);
        return (envelope, json);
    }

    #region 1. NotFoundException → 404

    /// <summary>
    /// For any random string message used in NotFoundException, the middleware always returns 404.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NotFoundException_WithAnyMessage_AlwaysReturns404()
    {
        return Prop.ForAll(
            Arb.From<NonEmptyString>(),
            message =>
            {
                var exception = new NotFoundException(message.Get);
                var middleware = CreateMiddleware(_ => throw exception);
                var context = CreateHttpContext();

                middleware.InvokeAsync(context).GetAwaiter().GetResult();

                context.Response.StatusCode.Should().Be(404,
                    $"NotFoundException with message '{message.Get}' should map to 404");
            });
    }

    #endregion

    #region 2. BusinessRuleException → 422

    /// <summary>
    /// For any random string message used in BusinessRuleException, the middleware always returns 422.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property BusinessRuleException_WithAnyMessage_AlwaysReturns422()
    {
        return Prop.ForAll(
            Arb.From<NonEmptyString>(),
            message =>
            {
                var exception = new BusinessRuleException(message.Get);
                var middleware = CreateMiddleware(_ => throw exception);
                var context = CreateHttpContext();

                middleware.InvokeAsync(context).GetAwaiter().GetResult();

                context.Response.StatusCode.Should().Be(422,
                    $"BusinessRuleException with message '{message.Get}' should map to 422");
            });
    }

    #endregion

    #region 3. GraphServiceException → 502 with no internal details

    /// <summary>
    /// For any random operation/reason strings in GraphServiceException, the middleware always
    /// returns 502 and no internal details (operation name, failure reason) leak in the response.
    /// Uses distinguishable prefix strings to avoid false positives from short common substrings.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property GraphServiceException_WithAnyOperationAndReason_AlwaysReturns502_NoDetailsLeak()
    {
        // Generate operation/reason strings with a distinguishable prefix to ensure they are
        // unique enough not to accidentally match parts of the fixed JSON response.
        var operationGen = Arb.From<NonEmptyString>().Generator
            .Select(s => "OP_" + s.Get + "_INTERNAL");
        var reasonGen = Arb.From<NonEmptyString>().Generator
            .Select(s => "REASON_" + s.Get + "_SECRET");

        return Prop.ForAll(
            operationGen.ToArbitrary(),
            reasonGen.ToArbitrary(),
            (operation, reason) =>
            {
                var exception = new GraphServiceException(operation, reason);
                var middleware = CreateMiddleware(_ => throw exception);
                var context = CreateHttpContext();

                middleware.InvokeAsync(context).GetAwaiter().GetResult();

                context.Response.StatusCode.Should().Be(502,
                    $"GraphServiceException should map to 502");

                var (envelope, rawJson) = GetResponseFromContext(context).GetAwaiter().GetResult();

                // Operation name and failure reason must not leak
                rawJson.Should().NotContain(operation,
                    "operation name should not leak in the response");
                rawJson.Should().NotContain(reason,
                    "failure reason should not leak in the response");
            });
    }

    #endregion

    #region 4. AuthenticationException → 401

    /// <summary>
    /// For any random message in AuthenticationException, the middleware always returns 401.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AuthenticationException_WithAnyMessage_AlwaysReturns401()
    {
        return Prop.ForAll(
            Arb.From<NonEmptyString>(),
            message =>
            {
                var exception = new AuthenticationException(message.Get);
                var middleware = CreateMiddleware(_ => throw exception);
                var context = CreateHttpContext();

                middleware.InvokeAsync(context).GetAwaiter().GetResult();

                context.Response.StatusCode.Should().Be(401,
                    $"AuthenticationException with message '{message.Get}' should map to 401");
            });
    }

    #endregion

    #region 5. Unknown exception types → 500

    /// <summary>
    /// For any random Exception subclass (various generated types), unknown types always return 500.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UnknownExceptionTypes_AlwaysReturn500()
    {
        // Generator that creates various unknown exception types with random messages
        var unknownExceptionGen = Arb.From<NonEmptyString>().Generator.SelectMany(msg =>
            Gen.Elements<Func<string, Exception>>(
                m => new InvalidOperationException(m),
                m => new ArgumentException(m),
                m => new NullReferenceException(m),
                m => new TimeoutException(m),
                m => new NotImplementedException(m),
                m => new IndexOutOfRangeException(m),
                m => new DivideByZeroException(m),
                m => new FormatException(m),
                m => new IOException(m),
                m => new ApplicationException(m)
            ).Select(factory => factory(msg.Get)));

        return Prop.ForAll(
            unknownExceptionGen.ToArbitrary(),
            exception =>
            {
                var middleware = CreateMiddleware(_ => throw exception);
                var context = CreateHttpContext();

                middleware.InvokeAsync(context).GetAwaiter().GetResult();

                context.Response.StatusCode.Should().Be(500,
                    $"Unknown exception type {exception.GetType().Name} should map to 500");
            });
    }

    #endregion

    #region 6. All exception types: valid API_Envelope with success=false, valid timestamp, valid correlationId, no stack traces

    /// <summary>
    /// For all exception types: the response is a valid API_Envelope with success=false,
    /// a valid ISO 8601 timestamp, a valid correlationId GUID, and no stack traces in raw JSON.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AllExceptionTypes_ReturnValidApiEnvelope_WithNoStackTraces()
    {
        // Generator that creates a mix of all known and unknown exception types
        var allExceptionGen = Arb.From<NonEmptyString>().Generator.SelectMany(msg =>
            Gen.OneOf(
                Gen.Constant<Exception>(new NotFoundException(msg.Get)),
                Gen.Constant<Exception>(new BusinessRuleException(msg.Get)),
                Gen.Constant<Exception>(new GraphServiceException("Op", msg.Get)),
                Gen.Constant<Exception>(new AuthenticationException(msg.Get)),
                Gen.Constant<Exception>(new InvalidOperationException(msg.Get)),
                Gen.Constant<Exception>(new NullReferenceException(msg.Get)),
                Gen.Constant<Exception>(new ArgumentException(msg.Get)),
                Gen.Constant<Exception>(new TimeoutException(msg.Get))
            ));

        return Prop.ForAll(
            allExceptionGen.ToArbitrary(),
            exception =>
            {
                var correlationId = Guid.NewGuid().ToString();
                var middleware = CreateMiddleware(_ => throw exception);
                var context = CreateHttpContext(correlationId);

                middleware.InvokeAsync(context).GetAwaiter().GetResult();

                var (envelope, rawJson) = GetResponseFromContext(context).GetAwaiter().GetResult();

                // Envelope must be non-null and deserialize correctly
                envelope.Should().NotBeNull("response must be a valid API_Envelope");

                // success must be false for all error responses
                envelope!.Success.Should().BeFalse("error responses must have success=false");

                // timestamp must be a valid ISO 8601 string
                envelope.Timestamp.Should().NotBeNullOrEmpty("timestamp must be present");
                DateTimeOffset.TryParse(envelope.Timestamp, out _).Should().BeTrue(
                    $"timestamp '{envelope.Timestamp}' must be a valid ISO 8601 date");

                // correlationId must be preserved and valid GUID
                envelope.CorrelationId.Should().Be(correlationId,
                    "correlationId must be preserved from the request context");
                Guid.TryParse(envelope.CorrelationId, out _).Should().BeTrue(
                    "correlationId must be a valid GUID");

                // No stack traces in raw JSON
                rawJson.Should().NotContain("StackTrace",
                    "no stack traces should appear in the response");
                rawJson.Should().NotContain("at System.",
                    "no .NET stack frame references should appear");
                rawJson.Should().NotContain("at GraphBridge.",
                    "no project stack frame references should appear");
            });
    }

    #endregion
}
