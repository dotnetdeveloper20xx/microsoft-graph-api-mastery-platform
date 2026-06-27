using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphPlannerService for Demo_Mode.
/// Returns deterministic, realistic task board and planner data without making any network calls.
/// </summary>
public class MockGraphPlannerService : IGraphPlannerService
{
    public Task<TaskBoardDto> CreateTaskBoardAsync(CreateTaskBoardRequest request, CancellationToken ct = default)
    {
        var taskBoard = new TaskBoardDto
        {
            Buckets = new List<TaskBucketDto>
            {
                new TaskBucketDto
                {
                    Name = "To Do",
                    Tasks = new List<ProjectTaskDto>
                    {
                        new ProjectTaskDto { Title = "Review planning documents", Status = "Not Started", AssignedTo = "Sarah Khan" },
                        new ProjectTaskDto { Title = "Update risk register", Status = "Not Started", AssignedTo = "James Wilson" },
                        new ProjectTaskDto { Title = "Schedule client meeting", Status = "Not Started", AssignedTo = "Emily Chen" }
                    }
                },
                new TaskBucketDto
                {
                    Name = "In Progress",
                    Tasks = new List<ProjectTaskDto>
                    {
                        new ProjectTaskDto { Title = "Draft contract amendments", Status = "In Progress", AssignedTo = "Michael Roberts" },
                        new ProjectTaskDto { Title = "Prepare site report", Status = "In Progress", AssignedTo = "Sarah Khan" }
                    }
                },
                new TaskBucketDto
                {
                    Name = "Completed",
                    Tasks = new List<ProjectTaskDto>
                    {
                        new ProjectTaskDto { Title = "Submit planning application", Status = "Completed", AssignedTo = "James Wilson" },
                        new ProjectTaskDto { Title = "Complete environmental assessment", Status = "Completed", AssignedTo = "Emily Chen" }
                    }
                }
            }
        };

        return Task.FromResult(taskBoard);
    }

    public Task<IReadOnlyList<PlannerTaskDto>> GetPendingTasksAsync(int limit = 50, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var tasks = new List<PlannerTaskDto>
        {
            new PlannerTaskDto
            {
                Id = "task-001",
                Title = "Review quarterly financial projections",
                Status = "Not Started",
                AssignedTo = "Sarah Khan",
                DueDate = now.AddDays(2)
            },
            new PlannerTaskDto
            {
                Id = "task-002",
                Title = "Finalise stakeholder presentation",
                Status = "In Progress",
                AssignedTo = "James Wilson",
                DueDate = now.AddDays(1)
            },
            new PlannerTaskDto
            {
                Id = "task-003",
                Title = "Submit compliance documentation",
                Status = "Overdue",
                AssignedTo = "Emily Chen",
                DueDate = now.AddDays(-1)
            },
            new PlannerTaskDto
            {
                Id = "task-004",
                Title = "Update project timeline",
                Status = "Not Started",
                AssignedTo = "Michael Roberts",
                DueDate = now.AddDays(5)
            },
            new PlannerTaskDto
            {
                Id = "task-005",
                Title = "Conduct site safety inspection",
                Status = "In Progress",
                AssignedTo = "David Thompson",
                DueDate = now.AddDays(3)
            },
            new PlannerTaskDto
            {
                Id = "task-006",
                Title = "Prepare budget variance report",
                Status = "Overdue",
                AssignedTo = "Sarah Khan",
                DueDate = now.AddDays(-2)
            },
            new PlannerTaskDto
            {
                Id = "task-007",
                Title = "Schedule team retrospective",
                Status = "Not Started",
                AssignedTo = "James Wilson",
                DueDate = now.AddDays(4)
            }
        };

        IReadOnlyList<PlannerTaskDto> result = tasks.Take(limit).ToList();
        return Task.FromResult(result);
    }

    public Task<TaskCompletionSummaryDto> GetTaskSummaryAsync(int days = 7, CancellationToken ct = default)
    {
        var summary = new TaskCompletionSummaryDto
        {
            Completed = 8,
            Overdue = 2,
            InProgress = 5
        };

        return Task.FromResult(summary);
    }
}
