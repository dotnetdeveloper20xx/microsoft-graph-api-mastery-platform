using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property-based tests verifying the API Envelope Structure Invariant (Property 1).
/// For any HTTP response from any API endpoint (success or failure), the response body
/// SHALL deserialize to a valid API_Envelope.
///
/// **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.6**
/// </summary>
public class ApiEnvelopeStructurePropertyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Known GET endpoints that should return successful responses in Demo_Mode.
    /// </summary>
    private static readonly string[] SuccessGetEndpoints =
    [
        "/api/onboarding/overview",
        "/api/legal-matters/overview",
        "/api/loan-approvals/overview",
        "/api/buildestate-projects/overview",
        "/api/ceo-command-centre/overview",
        "/api/productivity-assistant/overview",
        "/api/ceo-command-centre/today",
        "/api/ceo-command-centre/emails",
        "/api/ceo-command-centre/tasks",
        "/api/ceo-command-centre/documents",
        "/api/ceo-command-centre/security-signals",
        "/api/productivity-assistant/calendar",
        "/api/productivity-assistant/emails",
        "/api/productivity-assistant/tasks",
        "/api/productivity-assistant/documents",
        "/api/productivity-assistant/context-package"
    ];

    /// <summary>
    /// POST endpoints that create resources (need valid bodies to succeed).
    /// </summary>
    private static readonly string[] CreateEndpoints =
    [
        "/api/onboarding/employees",
        "/api/legal-matters",
        "/api/loan-approvals",
        "/api/buildestate-projects"
    ];

    public ApiEnvelopeStructurePropertyTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("GraphBridge:GraphMode", "Demo");
        }).CreateClient();
    }

    #region Property 1: Success responses deserialize to valid ApiEnvelope

    [Property(MaxTest = 100)]
    public Property SuccessResponses_AlwaysDeserializeToValidApiEnvelope()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                envelope.Should().NotBeNull($"Response from {endpoint} must deserialize to ApiEnvelope");
            });
    }

    #endregion

    #region Property 2: Success field is always boolean (implicitly tested by deserialization to bool)

    [Property(MaxTest = 100)]
    public Property SuccessField_IsAlwaysBoolean()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                root.TryGetProperty("success", out var successProp).Should().BeTrue(
                    $"Response from {endpoint} must have a 'success' property");
                (successProp.ValueKind == JsonValueKind.True || successProp.ValueKind == JsonValueKind.False)
                    .Should().BeTrue($"'success' from {endpoint} must be a boolean value");
            });
    }

    #endregion

    #region Property 3: Message is always ≤ 500 characters

    [Property(MaxTest = 100)]
    public Property Message_IsAlwaysWithin500Characters()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                envelope.Should().NotBeNull();
                envelope!.Message.Length.Should().BeLessOrEqualTo(500,
                    $"Message from {endpoint} must be at most 500 characters");
            });
    }

    #endregion

    #region Property 4: Errors array is never null (always at least empty)

    [Property(MaxTest = 100)]
    public Property ErrorsArray_IsNeverNull()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                envelope.Should().NotBeNull();
                envelope!.Errors.Should().NotBeNull(
                    $"Errors array from {endpoint} must never be null");
            });
    }

    #endregion

    #region Property 5: Timestamp is always valid ISO 8601 UTC

    [Property(MaxTest = 100)]
    public Property Timestamp_IsAlwaysValidIso8601Utc()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                envelope.Should().NotBeNull();
                envelope!.Timestamp.Should().NotBeNullOrEmpty(
                    $"Timestamp from {endpoint} must not be null or empty");

                var parsed = DateTimeOffset.TryParse(
                    envelope.Timestamp,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var dto);

                parsed.Should().BeTrue(
                    $"Timestamp '{envelope.Timestamp}' from {endpoint} must be valid ISO 8601");
                dto.Offset.Should().Be(TimeSpan.Zero,
                    $"Timestamp from {endpoint} must be UTC (offset zero)");
            });
    }

    #endregion

    #region Property 6: CorrelationId is always a valid GUID string

    [Property(MaxTest = 100)]
    public Property CorrelationId_IsAlwaysValidGuid()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                envelope.Should().NotBeNull();
                envelope!.CorrelationId.Should().NotBeNullOrEmpty(
                    $"CorrelationId from {endpoint} must not be null or empty");
                Guid.TryParse(envelope.CorrelationId, out _).Should().BeTrue(
                    $"CorrelationId '{envelope.CorrelationId}' from {endpoint} must be a valid GUID");
            });
    }

    #endregion

    #region Property 7: On success — success=true, data not null, errors empty

    [Property(MaxTest = 100)]
    public Property OnSuccess_SuccessTrueDataNotNullErrorsEmpty()
    {
        return Prop.ForAll(
            Gen.Elements(SuccessGetEndpoints).ToArbitrary(),
            endpoint =>
            {
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                envelope.Should().NotBeNull();

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    envelope!.Success.Should().BeTrue(
                        $"Success from {endpoint} with status {response.StatusCode} must be true");
                    envelope.Data.ValueKind.Should().NotBe(JsonValueKind.Undefined,
                        $"Data from {endpoint} on success must not be undefined");
                    envelope.Errors.Should().BeEmpty(
                        $"Errors from {endpoint} on success must be empty");
                }
            });
    }

    #endregion

    #region Property 8: On failure (400/404/500) — success=false

    [Property(MaxTest = 100)]
    public Property OnFailure_SuccessIsFalse_ForNotFoundRequests()
    {
        // Generate random GUIDs to hit non-existent resource endpoints → triggers 404
        return Prop.ForAll(
            Arb.From<Guid>(),
            randomGuid =>
            {
                var notFoundEndpoints = new[]
                {
                    $"/api/onboarding/employees/{randomGuid}",
                    $"/api/legal-matters/{randomGuid}",
                    $"/api/loan-approvals/{randomGuid}",
                    $"/api/buildestate-projects/{randomGuid}"
                };

                foreach (var endpoint in notFoundEndpoints)
                {
                    var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);
                    envelope.Should().NotBeNull();

                    if ((int)response.StatusCode >= 400)
                    {
                        envelope!.Success.Should().BeFalse(
                            $"Success from {endpoint} with status {response.StatusCode} must be false");
                    }
                }
            });
    }

    [Property(MaxTest = 100)]
    public Property OnFailure_EnvelopeFormatIsCorrect_ForErrorResponses()
    {
        // Use NOT FOUND scenarios (random GUIDs) which reliably go through our middleware
        // and produce proper ApiEnvelope error responses.
        return Prop.ForAll(
            Arb.From<Guid>(),
            randomGuid =>
            {
                var errorEndpoints = new[]
                {
                    $"/api/onboarding/employees/{randomGuid}",
                    $"/api/onboarding/employees/{randomGuid}/status",
                    $"/api/legal-matters/{randomGuid}",
                    $"/api/loan-approvals/{randomGuid}",
                    $"/api/buildestate-projects/{randomGuid}"
                };

                // Pick one endpoint per iteration to keep test fast
                var endpoint = errorEndpoints[Math.Abs(randomGuid.GetHashCode()) % errorEndpoints.Length];
                var response = _client.GetAsync(endpoint).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                // The response must be a valid ApiEnvelope even on failure
                envelope.Should().NotBeNull($"Error response from {endpoint} must deserialize to ApiEnvelope");
                envelope!.Success.Should().BeFalse(
                    $"Success from {endpoint} with 404 status must be false");
                envelope.Errors.Should().NotBeNull(
                    $"Errors from {endpoint} must never be null");
                envelope.CorrelationId.Should().NotBeNullOrEmpty();
                Guid.TryParse(envelope.CorrelationId, out _).Should().BeTrue(
                    $"CorrelationId from {endpoint} must be a valid GUID");
                envelope.Message.Length.Should().BeLessOrEqualTo(500);

                var parsed = DateTimeOffset.TryParse(
                    envelope.Timestamp,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out _);
                parsed.Should().BeTrue(
                    $"Timestamp from {endpoint} must be valid ISO 8601");
            });
    }

    #endregion

    #region Additional: Envelope structure is consistent across both success and error responses

    [Property(MaxTest = 100)]
    public Property AllResponses_HaveCompleteEnvelopeStructure()
    {
        // Combine success endpoints and error-triggering patterns
        var allEndpoints = SuccessGetEndpoints
            .Select(e => (Endpoint: e, Method: "GET", Body: (string)string.Empty))
            .Concat(CreateEndpoints.Select(e => (Endpoint: e, Method: "POST", Body: "{}")))
            .ToArray();

        return Prop.ForAll(
            Gen.Elements(allEndpoints).ToArbitrary(),
            testCase =>
            {
                HttpResponseMessage response;
                if (testCase.Method == "POST")
                {
                    var content = new StringContent(testCase.Body, Encoding.UTF8, "application/json");
                    response = _client.PostAsync(testCase.Endpoint, content).GetAwaiter().GetResult();
                }
                else
                {
                    response = _client.GetAsync(testCase.Endpoint).GetAwaiter().GetResult();
                }

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Verify all required envelope fields are present
                root.TryGetProperty("success", out _).Should().BeTrue(
                    $"Response from {testCase.Endpoint} must have 'success' field");
                root.TryGetProperty("message", out _).Should().BeTrue(
                    $"Response from {testCase.Endpoint} must have 'message' field");
                root.TryGetProperty("errors", out var errorsElement).Should().BeTrue(
                    $"Response from {testCase.Endpoint} must have 'errors' field");
                root.TryGetProperty("timestamp", out _).Should().BeTrue(
                    $"Response from {testCase.Endpoint} must have 'timestamp' field");
                root.TryGetProperty("correlationId", out _).Should().BeTrue(
                    $"Response from {testCase.Endpoint} must have 'correlationId' field");

                // Verify errors is always an array
                errorsElement.ValueKind.Should().Be(JsonValueKind.Array,
                    $"'errors' from {testCase.Endpoint} must be an array");

                // If errors array has entries, each must have 'field' and 'detail' strings
                foreach (var errorItem in errorsElement.EnumerateArray())
                {
                    errorItem.TryGetProperty("field", out var fieldProp).Should().BeTrue(
                        $"Each error in {testCase.Endpoint} must have 'field' property");
                    fieldProp.ValueKind.Should().Be(JsonValueKind.String,
                        $"Error 'field' in {testCase.Endpoint} must be a string");

                    errorItem.TryGetProperty("detail", out var detailProp).Should().BeTrue(
                        $"Each error in {testCase.Endpoint} must have 'detail' property");
                    detailProp.ValueKind.Should().Be(JsonValueKind.String,
                        $"Error 'detail' in {testCase.Endpoint} must be a string");
                }
            });
    }

    #endregion
}
