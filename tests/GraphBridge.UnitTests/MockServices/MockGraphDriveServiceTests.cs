using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphDriveService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphDriveServiceTests
{
    private readonly MockGraphDriveService _sut = new();

    [Fact]
    public async Task CreateFolderStructureAsync_ReturnsCompleteDto()
    {
        var request = new CreateFolderStructureRequest
        {
            WorkspaceId = "workspace-001",
            FolderNames = new List<string> { "Correspondence", "Contracts", "Evidence", "Notes" }
        };

        var result = await _sut.CreateFolderStructureAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrEmpty();
        result.Children.Should().NotBeNull();
        result.Children.Should().NotBeEmpty();
        foreach (var child in result.Children)
        {
            child.Name.Should().NotBeNullOrEmpty();
            child.Children.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetFolderStructureAsync_ReturnsCompleteDto()
    {
        var result = await _sut.GetFolderStructureAsync("workspace-001");

        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrEmpty();
        result.Children.Should().NotBeNull();
        result.Children.Should().NotBeEmpty();
        foreach (var child in result.Children)
        {
            child.Name.Should().NotBeNullOrEmpty();
            child.Children.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetRecentDocumentsAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetRecentDocumentsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var doc in result)
        {
            doc.Name.Should().NotBeNullOrEmpty();
            doc.ModifiedBy.Should().NotBeNullOrEmpty();
            doc.ModifiedAt.Should().NotBe(default);
            doc.Location.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetPendingApprovalsAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetPendingApprovalsAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var doc in result)
        {
            doc.Name.Should().NotBeNullOrEmpty();
            doc.ModifiedBy.Should().NotBeNullOrEmpty();
            doc.ModifiedAt.Should().NotBe(default);
            doc.Location.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphDriveService);
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
