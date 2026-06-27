using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphTeamsService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphTeamsServiceTests
{
    private readonly MockGraphTeamsService _sut = new();

    [Fact]
    public async Task CreateChannelAsync_ReturnsCompleteDto()
    {
        var request = new CreateChannelRequest
        {
            TeamId = "team-001",
            DisplayName = "Test Channel",
            Description = "A test channel for unit testing"
        };

        var result = await _sut.CreateChannelAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.DisplayName.Should().NotBeNullOrEmpty();
        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphTeamsService);
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
