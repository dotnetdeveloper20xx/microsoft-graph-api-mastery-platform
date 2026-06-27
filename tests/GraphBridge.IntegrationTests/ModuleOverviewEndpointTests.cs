using System.Globalization;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GraphBridge.IntegrationTests;

/// <summary>
/// Integration tests verifying that each module's overview endpoint returns
/// HTTP 200 with a valid API_Envelope containing success=true and non-null data
/// when running in Demo_Mode without any Microsoft Entra ID configuration.
///
/// Validates: Requirements 13.4, 3.5
/// </summary>
public class ModuleOverviewEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ModuleOverviewEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Configure WebApplicationFactory with Demo_Mode and explicitly remove Entra ID settings
        // to verify the application starts successfully without them (Requirement 3.5)
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("GraphBridge:GraphMode", "Demo");
            // Ensure no Entra ID configuration is present
            builder.UseSetting("GraphBridge:AzureAd:TenantId", "");
            builder.UseSetting("GraphBridge:AzureAd:ClientId", "");
            builder.UseSetting("GraphBridge:AzureAd:ClientSecret", "");
        }).CreateClient();
    }

    #region Application Startup Without Entra ID

    [Fact]
    public async Task Application_StartsSuccessfully_WithoutEntraIdConfiguration()
    {
        // Act — simply making any request proves the app started without Entra ID config
        var response = await _client.GetAsync("/api/onboarding/overview");

        // Assert — if we get here, the application started without Entra ID values
        response.Should().NotBeNull("Application should start successfully without Entra ID configuration");
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Application must respond successfully in Demo_Mode without Entra ID credentials");
    }

    #endregion

    #region Onboarding Module Overview

    [Fact]
    public async Task OnboardingOverview_ReturnsHttp200_WithValidEnvelope_AndNonNullData()
    {
        // Act
        var response = await _client.GetAsync("/api/onboarding/overview");

        // Assert
        await AssertValidOverviewResponse(response, "/api/onboarding/overview");
    }

    #endregion

    #region Legal Matters Module Overview

    [Fact]
    public async Task LegalMattersOverview_ReturnsHttp200_WithValidEnvelope_AndNonNullData()
    {
        // Act
        var response = await _client.GetAsync("/api/legal-matters/overview");

        // Assert
        await AssertValidOverviewResponse(response, "/api/legal-matters/overview");
    }

    #endregion

    #region Loan Approvals Module Overview

    [Fact]
    public async Task LoanApprovalsOverview_ReturnsHttp200_WithValidEnvelope_AndNonNullData()
    {
        // Act
        var response = await _client.GetAsync("/api/loan-approvals/overview");

        // Assert
        await AssertValidOverviewResponse(response, "/api/loan-approvals/overview");
    }

    #endregion

    #region BuildEstate Projects Module Overview

    [Fact]
    public async Task BuildEstateProjectsOverview_ReturnsHttp200_WithValidEnvelope_AndNonNullData()
    {
        // Act
        var response = await _client.GetAsync("/api/buildestate-projects/overview");

        // Assert
        await AssertValidOverviewResponse(response, "/api/buildestate-projects/overview");
    }

    #endregion

    #region CEO Command Centre Module Overview

    [Fact]
    public async Task CeoCommandCentreOverview_ReturnsHttp200_WithValidEnvelope_AndNonNullData()
    {
        // Act
        var response = await _client.GetAsync("/api/ceo-command-centre/overview");

        // Assert
        await AssertValidOverviewResponse(response, "/api/ceo-command-centre/overview");
    }

    #endregion

    #region Productivity Assistant Module Overview

    [Fact]
    public async Task ProductivityAssistantOverview_ReturnsHttp200_WithValidEnvelope_AndNonNullData()
    {
        // Act
        var response = await _client.GetAsync("/api/productivity-assistant/overview");

        // Assert
        await AssertValidOverviewResponse(response, "/api/productivity-assistant/overview");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Asserts that a response from an overview endpoint is HTTP 200 with a valid
    /// API_Envelope structure containing success=true, non-null data, and a valid correlationId.
    /// </summary>
    private static async Task AssertValidOverviewResponse(HttpResponseMessage response, string endpoint)
    {
        // 1. Verify HTTP 200 status code
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Overview endpoint {endpoint} should return HTTP 200 in Demo_Mode");

        // 2. Read and parse the response body
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty($"Response body from {endpoint} must not be empty");

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 3. Verify all API_Envelope fields are present
        root.TryGetProperty("success", out var successProp).Should().BeTrue(
            $"Response from {endpoint} must have 'success' field");
        root.TryGetProperty("message", out var messageProp).Should().BeTrue(
            $"Response from {endpoint} must have 'message' field");
        root.TryGetProperty("data", out var dataProp).Should().BeTrue(
            $"Response from {endpoint} must have 'data' field");
        root.TryGetProperty("errors", out var errorsProp).Should().BeTrue(
            $"Response from {endpoint} must have 'errors' field");
        root.TryGetProperty("timestamp", out var timestampProp).Should().BeTrue(
            $"Response from {endpoint} must have 'timestamp' field");
        root.TryGetProperty("correlationId", out var correlationIdProp).Should().BeTrue(
            $"Response from {endpoint} must have 'correlationId' field");

        // 4. Verify success is true
        successProp.GetBoolean().Should().BeTrue(
            $"Overview endpoint {endpoint} should have success=true in Demo_Mode");

        // 5. Verify message is a non-empty string within 500 characters
        messageProp.ValueKind.Should().Be(JsonValueKind.String,
            $"'message' from {endpoint} must be a string");
        var message = messageProp.GetString()!;
        message.Should().NotBeNullOrEmpty($"'message' from {endpoint} must not be empty");
        message.Length.Should().BeLessOrEqualTo(500,
            $"'message' from {endpoint} must be at most 500 characters");

        // 6. Verify data is non-null (not JsonValueKind.Null or Undefined)
        dataProp.ValueKind.Should().NotBe(JsonValueKind.Null,
            $"'data' from {endpoint} must not be null in Demo_Mode");
        dataProp.ValueKind.Should().NotBe(JsonValueKind.Undefined,
            $"'data' from {endpoint} must not be undefined");

        // 7. Verify errors is an empty array
        errorsProp.ValueKind.Should().Be(JsonValueKind.Array,
            $"'errors' from {endpoint} must be an array");
        errorsProp.GetArrayLength().Should().Be(0,
            $"'errors' from {endpoint} should be empty for successful responses");

        // 8. Verify timestamp is a valid ISO 8601 UTC string
        timestampProp.ValueKind.Should().Be(JsonValueKind.String,
            $"'timestamp' from {endpoint} must be a string");
        var timestamp = timestampProp.GetString()!;
        timestamp.Should().NotBeNullOrEmpty($"'timestamp' from {endpoint} must not be empty");
        var timestampParsed = DateTimeOffset.TryParse(
            timestamp,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsedTimestamp);
        timestampParsed.Should().BeTrue(
            $"'timestamp' value '{timestamp}' from {endpoint} must be valid ISO 8601");

        // 9. Verify correlationId is a valid GUID
        correlationIdProp.ValueKind.Should().Be(JsonValueKind.String,
            $"'correlationId' from {endpoint} must be a string");
        var correlationId = correlationIdProp.GetString()!;
        correlationId.Should().NotBeNullOrEmpty(
            $"'correlationId' from {endpoint} must not be empty");
        Guid.TryParse(correlationId, out var parsedGuid).Should().BeTrue(
            $"'correlationId' value '{correlationId}' from {endpoint} must be a valid GUID");
        parsedGuid.Should().NotBe(Guid.Empty,
            $"'correlationId' from {endpoint} must not be an empty GUID");
    }

    #endregion
}
