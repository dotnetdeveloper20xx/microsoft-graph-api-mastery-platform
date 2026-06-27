using FluentAssertions;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Infrastructure.MockServices;

namespace GraphBridge.UnitTests.MockServices;

/// <summary>
/// Verifies MockGraphPlannerService returns complete DTOs with non-null, non-empty values
/// and makes no network calls (no HttpClient dependency).
/// </summary>
public class MockGraphPlannerServiceTests
{
    private readonly MockGraphPlannerService _sut = new();

    [Fact]
    public async Task CreateTaskBoardAsync_ReturnsCompleteDto()
    {
        var request = new CreateTaskBoardRequest
        {
            PlanId = "plan-001",
            Title = "Test Board",
            BucketNames = new List<string> { "To Do", "In Progress", "Done" }
        };

        var result = await _sut.CreateTaskBoardAsync(request);

        result.Should().NotBeNull();
        result.Buckets.Should().NotBeNull();
        result.Buckets.Should().NotBeEmpty();
        foreach (var bucket in result.Buckets)
        {
            bucket.Name.Should().NotBeNullOrEmpty();
            bucket.Tasks.Should().NotBeNull();
            bucket.Tasks.Should().NotBeEmpty();
            foreach (var task in bucket.Tasks)
            {
                task.Title.Should().NotBeNullOrEmpty();
                task.Status.Should().NotBeNullOrEmpty();
                task.AssignedTo.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Fact]
    public async Task GetPendingTasksAsync_ReturnsPopulatedListWithCompleteDtos()
    {
        var result = await _sut.GetPendingTasksAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        foreach (var task in result)
        {
            task.Id.Should().NotBeNullOrEmpty();
            task.Title.Should().NotBeNullOrEmpty();
            task.Status.Should().NotBeNullOrEmpty();
            task.AssignedTo.Should().NotBeNullOrEmpty();
            task.DueDate.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetTaskSummaryAsync_ReturnsCompleteDto()
    {
        var result = await _sut.GetTaskSummaryAsync();

        result.Should().NotBeNull();
        // At least one category should have a positive value
        (result.Completed + result.Overdue + result.InProgress).Should().BeGreaterThan(0);
    }

    [Fact]
    public void DoesNotUseHttpClient()
    {
        var serviceType = typeof(MockGraphPlannerService);
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
