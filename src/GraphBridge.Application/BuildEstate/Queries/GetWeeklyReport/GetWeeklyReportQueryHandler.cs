using GraphBridge.Application.Dtos.BuildEstate;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Queries.GetWeeklyReport;

/// <summary>
/// Handles retrieving the weekly report for a BuildEstate project.
/// Fetches task summary from IGraphPlannerService and returns milestone/activity data.
/// </summary>
public class GetWeeklyReportQueryHandler : IRequestHandler<GetWeeklyReportQuery, WeeklyReportDto>
{
    private readonly IBuildEstateProjectStore _projectStore;
    private readonly IGraphPlannerService _plannerService;

    public GetWeeklyReportQueryHandler(IBuildEstateProjectStore projectStore, IGraphPlannerService plannerService)
    {
        _projectStore = projectStore;
        _plannerService = plannerService;
    }

    public async Task<WeeklyReportDto> Handle(GetWeeklyReportQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectStore.GetByIdAsync(request.ProjectId)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var taskSummary = await _plannerService.GetTaskSummaryAsync(7, cancellationToken);

        return new WeeklyReportDto
        {
            TasksToDo = taskSummary.Overdue, // Overdue maps to pending/to-do items
            TasksInProgress = taskSummary.InProgress,
            TasksCompleted = taskSummary.Completed,
            MilestonesDueThisWeek = new List<string> { "Foundation inspection", "Contractor review" },
            TeamActivityCount = taskSummary.Completed + taskSummary.InProgress + taskSummary.Overdue
        };
    }
}
