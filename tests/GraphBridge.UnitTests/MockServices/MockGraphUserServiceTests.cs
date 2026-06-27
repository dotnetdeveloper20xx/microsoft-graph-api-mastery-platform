using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphUserService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphUserServiceTests
{
    private readonly MockGraphUserService _sut = new();

    [Fact]
    public async Task GetUserProfileAsync_ReturnsCompleteDto()
    {
        var result = await _sut.GetUserProfileAsync("usr-001-sarah-khan");

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.DisplayName.Should().NotBeNullOrEmpty();
        result.Email.Should().NotBeNullOrEmpty();
        result.Department.Should().NotBeNullOrEmpty();
        result.JobTitle.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsCompleteDto()
    {
        var request = new CreateUserRequest
        {
            DisplayName = "Test User",
            Email = "test@graphbridge.dev",
            Department = "Engineering",
            JobTitle = "Developer"
        };

        var result = await _sut.CreateUserAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.DisplayName.Should().NotBeNullOrEmpty();
        result.Email.Should().NotBeNullOrEmpty();
        result.Department.Should().NotBeNullOrEmpty();
        result.JobTitle.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetUsersAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var user in result)
        {
            user.Id.Should().NotBeNullOrEmpty();
            user.DisplayName.Should().NotBeNullOrEmpty();
            user.Email.Should().NotBeNullOrEmpty();
            user.Department.Should().NotBeNullOrEmpty();
            user.JobTitle.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphUserService);
        var constructors = serviceType.GetConstructors();

        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            parameters.Should().NotContain(p => p.ParameterType == typeof(HttpClient),
                "mock services must not use HttpClient");
        }

        var fields = serviceType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
        fields.Should().NotContain(f => f.FieldType == typeof(HttpClient),
            "mock services must not have HttpClient fields");
    }
}
