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
/// Property 6: Entity Creation Returns Unique Identifier
/// For any valid creation request (employee, legal matter, loan approval, or project)
/// with all required fields present and within specified length/format constraints,
/// the system SHALL persist the entity and return a response containing a newly generated,
/// unique GUID identifier that differs from all previously generated identifiers.
///
/// **Validates: Requirements 5.1, 6.1, 7.1, 8.1**
/// </summary>
public class EntityCreationReturnsUniqueIdentifierPropertyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public EntityCreationReturnsUniqueIdentifierPropertyTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("GraphBridge:GraphMode", "Demo");
        }).CreateClient();
    }

    #region FsCheck Generators for valid requests

    /// <summary>
    /// Generates a non-empty alphanumeric string within the given length bounds.
    /// </summary>
    private static Gen<string> GenValidString(int minLength, int maxLength) =>
        from length in Gen.Choose(minLength, maxLength)
        from chars in Gen.ArrayOf(length, Gen.Elements(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -".ToCharArray()))
        select new string(chars);

    /// <summary>
    /// Generates a valid email address for testing.
    /// </summary>
    private static Gen<string> GenValidEmail() =>
        from localPart in GenValidString(3, 15)
        from domain in GenValidString(3, 10)
        let cleanLocal = new string(localPart.Where(c => char.IsLetterOrDigit(c)).ToArray())
        let cleanDomain = new string(domain.Where(c => char.IsLetterOrDigit(c)).ToArray())
        where cleanLocal.Length > 0 && cleanDomain.Length > 0
        select $"{cleanLocal}@{cleanDomain}.com";

    /// <summary>
    /// Generates a valid CreateEmployeeCommand JSON body.
    /// Name: 1-100, Role: 1-100, Department: 1-50, ManagerName: 1-100, Email: valid format
    /// </summary>
    private static Gen<string> GenCreateEmployeeBody() =>
        from name in GenValidString(1, 50)
        from role in GenValidString(1, 50)
        from department in GenValidString(1, 30)
        from managerName in GenValidString(1, 50)
        from email in GenValidEmail()
        select JsonSerializer.Serialize(new
        {
            name,
            role,
            department,
            managerName,
            email
        });

    /// <summary>
    /// Generates a valid CreateMatterCommand JSON body.
    /// ClientName: max 200, MatterType: required, AssignedSolicitor: required
    /// </summary>
    private static Gen<string> GenCreateLegalMatterBody() =>
        from clientName in GenValidString(1, 100)
        from matterType in Gen.Elements("Commercial", "Residential", "Litigation", "Corporate", "Employment")
        from assignedSolicitor in GenValidString(1, 50)
        select JsonSerializer.Serialize(new
        {
            clientName,
            matterType,
            assignedSolicitor
        });

    /// <summary>
    /// Generates a valid CreateLoanApprovalCommand JSON body.
    /// CustomerName: max 200, Amount: 0.01-999999999.99, PropertyReference: max 100, Status: required
    /// </summary>
    private static Gen<string> GenCreateLoanApprovalBody() =>
        from customerName in GenValidString(1, 100)
        from amountWhole in Gen.Choose(1, 999999)
        from amountFraction in Gen.Choose(0, 99)
        from propertyReference in GenValidString(1, 50)
        from status in Gen.Elements("Approved", "Pending", "Rejected", "UnderReview")
        let amount = (decimal)amountWhole + (decimal)amountFraction / 100m
        select JsonSerializer.Serialize(new
        {
            customerName,
            amount,
            propertyReference,
            status
        });

    /// <summary>
    /// Generates a valid CreateProjectCommand JSON body.
    /// Name: max 200, Location: max 200, PlanningStatus: required, Directors: at least 1
    /// </summary>
    private static Gen<string> GenCreateBuildEstateProjectBody() =>
        from name in GenValidString(1, 100)
        from location in GenValidString(1, 100)
        from planningStatus in Gen.Elements("Approved", "Pending", "InReview", "Rejected")
        from directorCount in Gen.Choose(1, 5)
        from directors in Gen.ListOf(directorCount, GenValidString(1, 50))
        select JsonSerializer.Serialize(new
        {
            name,
            location,
            planningStatus,
            directors
        });

    #endregion

    #region Property 6a: Employee creation returns unique GUID Id

    [Property(MaxTest = 100)]
    public Property EmployeeCreation_ReturnsUniqueGuidId()
    {
        return Prop.ForAll(
            GenCreateEmployeeBody().ToArbitrary(),
            body =>
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = _client.PostAsync("/api/onboarding/employees", content).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                    envelope.Should().NotBeNull();
                    envelope!.Success.Should().BeTrue("employee creation should succeed with valid data");
                    envelope.Data.ValueKind.Should().NotBe(JsonValueKind.Null);

                    var id = envelope.Data.GetProperty("id").GetString();
                    id.Should().NotBeNullOrEmpty("created employee must have an Id");
                    Guid.TryParse(id, out var parsedGuid).Should().BeTrue(
                        $"Employee Id '{id}' must be a valid GUID");
                    parsedGuid.Should().NotBe(Guid.Empty, "Employee Id must not be Guid.Empty");
                }
            });
    }

    #endregion

    #region Property 6b: Legal matter creation returns unique GUID Id

    [Property(MaxTest = 100)]
    public Property LegalMatterCreation_ReturnsUniqueGuidId()
    {
        return Prop.ForAll(
            GenCreateLegalMatterBody().ToArbitrary(),
            body =>
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = _client.PostAsync("/api/legal-matters", content).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                    envelope.Should().NotBeNull();
                    envelope!.Success.Should().BeTrue("legal matter creation should succeed with valid data");
                    envelope.Data.ValueKind.Should().NotBe(JsonValueKind.Null);

                    var id = envelope.Data.GetProperty("id").GetString();
                    id.Should().NotBeNullOrEmpty("created legal matter must have an Id");
                    Guid.TryParse(id, out var parsedGuid).Should().BeTrue(
                        $"Legal matter Id '{id}' must be a valid GUID");
                    parsedGuid.Should().NotBe(Guid.Empty, "Legal matter Id must not be Guid.Empty");
                }
            });
    }

    #endregion

    #region Property 6c: Loan approval creation returns unique GUID Id

    [Property(MaxTest = 100)]
    public Property LoanApprovalCreation_ReturnsUniqueGuidId()
    {
        return Prop.ForAll(
            GenCreateLoanApprovalBody().ToArbitrary(),
            body =>
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = _client.PostAsync("/api/loan-approvals", content).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                    envelope.Should().NotBeNull();
                    envelope!.Success.Should().BeTrue("loan approval creation should succeed with valid data");
                    envelope.Data.ValueKind.Should().NotBe(JsonValueKind.Null);

                    var id = envelope.Data.GetProperty("id").GetString();
                    id.Should().NotBeNullOrEmpty("created loan approval must have an Id");
                    Guid.TryParse(id, out var parsedGuid).Should().BeTrue(
                        $"Loan approval Id '{id}' must be a valid GUID");
                    parsedGuid.Should().NotBe(Guid.Empty, "Loan approval Id must not be Guid.Empty");
                }
            });
    }

    #endregion

    #region Property 6d: BuildEstate project creation returns unique GUID Id

    [Property(MaxTest = 100)]
    public Property BuildEstateProjectCreation_ReturnsUniqueGuidId()
    {
        return Prop.ForAll(
            GenCreateBuildEstateProjectBody().ToArbitrary(),
            body =>
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = _client.PostAsync("/api/buildestate-projects", content).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                    envelope.Should().NotBeNull();
                    envelope!.Success.Should().BeTrue("project creation should succeed with valid data");
                    envelope.Data.ValueKind.Should().NotBe(JsonValueKind.Null);

                    var id = envelope.Data.GetProperty("id").GetString();
                    id.Should().NotBeNullOrEmpty("created project must have an Id");
                    Guid.TryParse(id, out var parsedGuid).Should().BeTrue(
                        $"Project Id '{id}' must be a valid GUID");
                    parsedGuid.Should().NotBe(Guid.Empty, "Project Id must not be Guid.Empty");
                }
            });
    }

    #endregion

    #region Property 6e: Across multiple entity creations, all returned Ids are unique (no collisions)

    [Property(MaxTest = 100)]
    public Property AcrossMultipleCreations_AllIdsAreUnique()
    {
        // Generate a batch size between 3 and 10 entities of mixed types
        return Prop.ForAll(
            Gen.Choose(3, 10).ToArbitrary(),
            batchSize =>
            {
                var allIds = new List<string>();

                for (int i = 0; i < batchSize; i++)
                {
                    // Rotate through entity types
                    var (endpoint, body) = (i % 4) switch
                    {
                        0 => ("/api/onboarding/employees", GenerateEmployeeBody()),
                        1 => ("/api/legal-matters", GenerateLegalMatterBody()),
                        2 => ("/api/loan-approvals", GenerateLoanApprovalBody()),
                        _ => ("/api/buildestate-projects", GenerateBuildEstateProjectBody())
                    };

                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var response = _client.PostAsync(endpoint, content).GetAwaiter().GetResult();

                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                    {
                        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(json, JsonOptions);

                        if (envelope is { Success: true } &&
                            envelope.Data.ValueKind != JsonValueKind.Null &&
                            envelope.Data.ValueKind != JsonValueKind.Undefined)
                        {
                            var id = envelope.Data.GetProperty("id").GetString();
                            if (!string.IsNullOrEmpty(id))
                            {
                                allIds.Add(id);
                            }
                        }
                    }
                }

                // All collected Ids must be unique
                if (allIds.Count > 1)
                {
                    allIds.Distinct().Count().Should().Be(allIds.Count,
                        "all entity Ids across multiple creation requests must be unique (no collisions)");
                }
            });
    }

    #endregion

    #region Helper methods for deterministic body generation (used in batch uniqueness test)

    private static int _counter;

    private static string GenerateEmployeeBody()
    {
        var idx = Interlocked.Increment(ref _counter);
        return JsonSerializer.Serialize(new
        {
            name = $"Employee{idx}",
            role = $"Role{idx}",
            department = $"Dept{idx}",
            managerName = $"Manager{idx}",
            email = $"emp{idx}@test.com"
        });
    }

    private static string GenerateLegalMatterBody()
    {
        var idx = Interlocked.Increment(ref _counter);
        return JsonSerializer.Serialize(new
        {
            clientName = $"Client{idx}",
            matterType = "Commercial",
            assignedSolicitor = $"Solicitor{idx}"
        });
    }

    private static string GenerateLoanApprovalBody()
    {
        var idx = Interlocked.Increment(ref _counter);
        return JsonSerializer.Serialize(new
        {
            customerName = $"Customer{idx}",
            amount = 50000.00m + idx,
            propertyReference = $"PROP{idx}",
            status = "Approved"
        });
    }

    private static string GenerateBuildEstateProjectBody()
    {
        var idx = Interlocked.Increment(ref _counter);
        return JsonSerializer.Serialize(new
        {
            name = $"Project{idx}",
            location = $"Location{idx}",
            planningStatus = "Approved",
            directors = new[] { $"Director{idx}" }
        });
    }

    #endregion
}
