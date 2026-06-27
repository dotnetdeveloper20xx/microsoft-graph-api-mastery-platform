using FluentAssertions;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphReportService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphReportServiceTests
{
    private readonly MockGraphReportService _sut = new();

    [Fact]
    public async Task GetActivityReportAsync_ReturnsCompleteDto()
    {
        var result = await _sut.GetActivityReportAsync();

        result.Should().NotBeNull();
        result.TotalActivities.Should().BeGreaterThan(0);
        result.ActiveUsers.Should().BeGreaterThan(0);
        result.ReportDate.Should().NotBe(default);
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphReportService);
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
