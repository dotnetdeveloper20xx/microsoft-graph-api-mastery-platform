using FluentAssertions;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphSecurityService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphSecurityServiceTests
{
    private readonly MockGraphSecurityService _sut = new();

    [Fact]
    public async Task GetRecentAlertsAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetRecentAlertsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var alert in result)
        {
            alert.Title.Should().NotBeNullOrEmpty();
            alert.Severity.Should().NotBeNullOrEmpty();
            alert.DetectedAt.Should().NotBe(default);
            alert.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphSecurityService);
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
