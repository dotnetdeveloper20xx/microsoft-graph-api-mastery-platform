using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GraphBridge.IntegrationTests;

/// <summary>
/// Integration tests for validation round-trips, not-found handling, full module workflows,
/// and correlationId propagation.
///
/// Validates: Requirements 13.4, 13.5
/// </summary>
public class ValidationNotFoundAndWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ValidationNotFoundAndWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("GraphBridge:GraphMode", "Demo");
            builder.UseSetting("GraphBridge:AzureAd:TenantId", "");
            builder.UseSetting("GraphBridge:AzureAd:ClientId", "");
            builder.UseSetting("GraphBridge:AzureAd:ClientSecret", "");
        }).CreateClient();
    }

    #region Validation Round-Trip Tests

    [Fact]
    public async Task CreateEmployee_WithEmptyFields_Returns400_WithFieldErrors()
    {
        // Arrange — empty required fields
        var payload = new { Name = "", Role = "", Department = "", ManagerName = "", Email = "" };
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/onboarding/employees", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().Be("Validation failed");

        var errors = envelope.GetProperty("errors");
        errors.ValueKind.Should().Be(JsonValueKind.Array);
        errors.GetArrayLength().Should().BeGreaterThan(0);

        // Should contain error entries with field and detail properties
        foreach (var error in errors.EnumerateArray())
        {
            error.TryGetProperty("field", out _).Should().BeTrue();
            error.TryGetProperty("detail", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task CreateEmployee_WithInvalidEmail_Returns400_WithEmailFieldError()
    {
        // Arrange — valid fields except invalid email format
        var payload = new
        {
            Name = "Test Employee",
            Role = "Developer",
            Department = "Engineering",
            ManagerName = "Manager One",
            Email = "not-a-valid-email"
        };
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/onboarding/employees", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().Be("Validation failed");

        var errors = envelope.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        // At least one error should reference the Email field
        var errorFields = errors.EnumerateArray()
            .Select(e => e.GetProperty("field").GetString()?.ToLowerInvariant())
            .ToList();
        errorFields.Should().Contain(f => f != null && f.Contains("email"));
    }

    [Fact]
    public async Task CreateLoanApproval_WithZeroAmount_Returns400_WithAmountFieldError()
    {
        // Arrange — Amount = 0 violates the 0.01 minimum
        var payload = new
        {
            CustomerName = "Test Customer",
            Amount = 0,
            PropertyReference = "REF-001",
            Status = "Approved"
        };
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/loan-approvals", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().Be("Validation failed");

        var errors = envelope.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var errorFields = errors.EnumerateArray()
            .Select(e => e.GetProperty("field").GetString()?.ToLowerInvariant())
            .ToList();
        errorFields.Should().Contain(f => f != null && f.Contains("amount"));
    }

    [Fact]
    public async Task CreateBuildEstateProject_WithEmptyDirectors_Returns400_WithDirectorsFieldError()
    {
        // Arrange — Directors list is empty (requires at least 1)
        var payload = new
        {
            Name = "Test Project",
            Location = "London",
            PlanningStatus = "Approved",
            Directors = new List<string>()
        };
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/buildestate-projects", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().Be("Validation failed");

        var errors = envelope.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);

        var errorFields = errors.EnumerateArray()
            .Select(e => e.GetProperty("field").GetString()?.ToLowerInvariant())
            .ToList();
        errorFields.Should().Contain(f => f != null && f.Contains("director"));
    }

    [Fact]
    public async Task CreateLegalMatter_WithEmptyFields_Returns400_WithFieldErrors()
    {
        // Arrange — empty required fields
        var payload = new { ClientName = "", MatterType = "", AssignedSolicitor = "" };
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/legal-matters", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().Be("Validation failed");
        envelope.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    #endregion

    #region Not-Found Handling Tests

    [Fact]
    public async Task GetEmployee_WithNonExistentGuid_Returns404_WithEnvelope()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/onboarding/employees/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task GetLegalMatter_WithNonExistentGuid_Returns404_WithEnvelope()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/legal-matters/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        envelope.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task GetLoanApproval_WithNonExistentGuid_Returns404_WithEnvelope()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/loan-approvals/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task GetBuildEstateProject_WithNonExistentGuid_Returns404_WithEnvelope()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/buildestate-projects/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task AssignGroups_WithNonExistentEmployee_Returns404_WithEnvelope()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/onboarding/employees/{nonExistentId}/assign-groups", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await ParseEnvelope(response);
        envelope.GetProperty("success").GetBoolean().Should().BeFalse();
        AssertValidCorrelationId(envelope);
    }

    #endregion

    #region CorrelationId Propagation Tests

    [Fact]
    public async Task SuccessResponse_ContainsValidGuid_CorrelationId()
    {
        // Act
        var response = await _client.GetAsync("/api/onboarding/overview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ParseEnvelope(response);
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task ErrorResponse_ContainsValidGuid_CorrelationId()
    {
        // Arrange — trigger a 400 validation error
        var payload = new { Name = "", Role = "", Department = "", ManagerName = "", Email = "" };
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/onboarding/employees", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ParseEnvelope(response);
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task NotFoundResponse_ContainsValidGuid_CorrelationId()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/onboarding/employees/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await ParseEnvelope(response);
        AssertValidCorrelationId(envelope);
    }

    [Fact]
    public async Task MultipleRequests_ReturnDifferentCorrelationIds()
    {
        // Act
        var response1 = await _client.GetAsync("/api/onboarding/overview");
        var response2 = await _client.GetAsync("/api/onboarding/overview");

        // Assert
        var envelope1 = await ParseEnvelope(response1);
        var envelope2 = await ParseEnvelope(response2);

        var correlationId1 = envelope1.GetProperty("correlationId").GetString();
        var correlationId2 = envelope2.GetProperty("correlationId").GetString();

        correlationId1.Should().NotBe(correlationId2, "Each request must get a unique correlationId");
    }

    #endregion

    #region Full Workflow Tests — Onboarding Module

    [Fact]
    public async Task OnboardingWorkflow_CreateEmployee_AssignGroups_SendEmail_ScheduleInduction_VerifyStatus()
    {
        // Step 1: Create employee
        var createPayload = new
        {
            Name = "Integration Test Employee",
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Jane Manager",
            Email = "integration.test@example.com"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createPayload, JsonOptions), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/onboarding/employees", createContent);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createEnvelope = await ParseEnvelope(createResponse);
        createEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        var employeeData = createEnvelope.GetProperty("data");
        var employeeId = employeeData.GetProperty("id").GetString();
        employeeId.Should().NotBeNullOrEmpty();
        Guid.TryParse(employeeId, out _).Should().BeTrue();

        // Step 2: Assign groups
        var assignResponse = await _client.PostAsync($"/api/onboarding/employees/{employeeId}/assign-groups", null);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignEnvelope = await ParseEnvelope(assignResponse);
        assignEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        // Step 3: Send welcome email
        var emailResponse = await _client.PostAsync($"/api/onboarding/employees/{employeeId}/send-welcome-email", null);
        emailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var emailEnvelope = await ParseEnvelope(emailResponse);
        emailEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        // Step 4: Schedule induction
        var inductionResponse = await _client.PostAsync($"/api/onboarding/employees/{employeeId}/schedule-induction", null);
        inductionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var inductionEnvelope = await ParseEnvelope(inductionResponse);
        inductionEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        // Step 5: Verify status shows all steps completed
        var statusResponse = await _client.GetAsync($"/api/onboarding/employees/{employeeId}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusEnvelope = await ParseEnvelope(statusResponse);
        statusEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        var statusData = statusEnvelope.GetProperty("data");
        statusData.GetProperty("profileCreated").GetBoolean().Should().BeTrue();
        statusData.GetProperty("groupsAssigned").GetBoolean().Should().BeTrue();
        statusData.GetProperty("welcomeEmailSent").GetBoolean().Should().BeTrue();
        statusData.GetProperty("inductionScheduled").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region Full Workflow Tests — Legal Matters Module

    [Fact]
    public async Task LegalMatterWorkflow_CreateMatter_CreateWorkspace_InviteParticipants_ScheduleKickoff()
    {
        // Step 1: Create legal matter
        var createPayload = new
        {
            ClientName = "Workflow Test Client",
            MatterType = "Litigation",
            AssignedSolicitor = "John Solicitor"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createPayload, JsonOptions), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/legal-matters", createContent);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createEnvelope = await ParseEnvelope(createResponse);
        createEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        var matterData = createEnvelope.GetProperty("data");
        var matterId = matterData.GetProperty("id").GetString();
        matterId.Should().NotBeNullOrEmpty();
        Guid.TryParse(matterId, out _).Should().BeTrue();

        // Step 2: Create workspace
        var workspaceResponse = await _client.PostAsync($"/api/legal-matters/{matterId}/create-workspace", null);
        workspaceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var workspaceEnvelope = await ParseEnvelope(workspaceResponse);
        workspaceEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        // Step 3: Invite participants
        var invitePayload = new { Participants = new[] { "participant1@example.com", "participant2@example.com" } };
        var inviteContent = new StringContent(JsonSerializer.Serialize(invitePayload, JsonOptions), Encoding.UTF8, "application/json");
        var inviteResponse = await _client.PostAsync($"/api/legal-matters/{matterId}/invite-participants", inviteContent);
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var inviteEnvelope = await ParseEnvelope(inviteResponse);
        inviteEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();

        // Step 4: Schedule kickoff
        var kickoffResponse = await _client.PostAsync($"/api/legal-matters/{matterId}/schedule-kickoff", null);
        kickoffResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var kickoffEnvelope = await ParseEnvelope(kickoffResponse);
        kickoffEnvelope.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region BusinessRuleException Tests

    [Fact]
    public async Task LegalMatter_CreateWorkspaceTwice_ReturnsError()
    {
        // Step 1: Create legal matter
        var createPayload = new
        {
            ClientName = "Duplicate Workspace Client",
            MatterType = "Corporate",
            AssignedSolicitor = "Alice Solicitor"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createPayload, JsonOptions), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/legal-matters", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createEnvelope = await ParseEnvelope(createResponse);
        var matterId = createEnvelope.GetProperty("data").GetProperty("id").GetString();

        // Step 2: Create workspace first time — should succeed
        var firstResponse = await _client.PostAsync($"/api/legal-matters/{matterId}/create-workspace", null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Create workspace second time — should return error (422 BusinessRuleException)
        var secondResponse = await _client.PostAsync($"/api/legal-matters/{matterId}/create-workspace", null);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var errorEnvelope = await ParseEnvelope(secondResponse);
        errorEnvelope.GetProperty("success").GetBoolean().Should().BeFalse();
        errorEnvelope.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
        AssertValidCorrelationId(errorEnvelope);
    }

    [Fact]
    public async Task LoanApproval_SendCustomerEmail_BeforePackGenerated_ReturnsError()
    {
        // Step 1: Create approved loan
        var createPayload = new
        {
            CustomerName = "Pack Order Test Customer",
            Amount = 250000.00m,
            PropertyReference = "REF-ORDER-001",
            Status = "Approved"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createPayload, JsonOptions), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/loan-approvals", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createEnvelope = await ParseEnvelope(createResponse);
        var loanId = createEnvelope.GetProperty("data").GetProperty("id").GetString();

        // Step 2: Try to send customer email before generating pack — should return error
        var emailResponse = await _client.PostAsync($"/api/loan-approvals/{loanId}/send-customer-email", null);
        emailResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var errorEnvelope = await ParseEnvelope(emailResponse);
        errorEnvelope.GetProperty("success").GetBoolean().Should().BeFalse();
        errorEnvelope.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
        AssertValidCorrelationId(errorEnvelope);
    }

    [Fact]
    public async Task LoanApproval_GeneratePack_WithNonApprovedStatus_ReturnsError()
    {
        // Step 1: Create loan with non-approved status
        var createPayload = new
        {
            CustomerName = "Non Approved Customer",
            Amount = 100000.00m,
            PropertyReference = "REF-PENDING-001",
            Status = "Pending"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createPayload, JsonOptions), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/loan-approvals", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createEnvelope = await ParseEnvelope(createResponse);
        var loanId = createEnvelope.GetProperty("data").GetProperty("id").GetString();

        // Step 2: Try to generate pack for non-approved loan — should fail
        var packResponse = await _client.PostAsync($"/api/loan-approvals/{loanId}/generate-pack", null);
        packResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var errorEnvelope = await ParseEnvelope(packResponse);
        errorEnvelope.GetProperty("success").GetBoolean().Should().BeFalse();
        AssertValidCorrelationId(errorEnvelope);
    }

    #endregion

    #region Helper Methods

    private static async Task<JsonElement> ParseEnvelope(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement;
    }

    private static void AssertValidCorrelationId(JsonElement envelope)
    {
        envelope.TryGetProperty("correlationId", out var correlationIdProp).Should().BeTrue(
            "Response must contain 'correlationId' field");
        correlationIdProp.ValueKind.Should().Be(JsonValueKind.String);
        var correlationId = correlationIdProp.GetString();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out var parsed).Should().BeTrue(
            $"correlationId '{correlationId}' must be a valid GUID");
        parsed.Should().NotBe(Guid.Empty);
    }

    #endregion
}
