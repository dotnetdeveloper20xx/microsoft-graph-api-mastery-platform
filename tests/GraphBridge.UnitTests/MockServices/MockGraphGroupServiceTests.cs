using FluentAssertions;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphGroupService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphGroupServiceTests
{
    private readonly MockGraphGroupService _sut = new();

    [Fact]
    public async Task GetGroupsForDepartmentAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetGroupsForDepartmentAsync("HR");

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var group in result)
        {
            group.Id.Should().NotBeNullOrEmpty();
            group.DisplayName.Should().NotBeNullOrEmpty();
            group.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetUserGroupsAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetUserGroupsAsync("usr-001");

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var group in result)
        {
            group.Id.Should().NotBeNullOrEmpty();
            group.DisplayName.Should().NotBeNullOrEmpty();
            group.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphGroupService);
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
