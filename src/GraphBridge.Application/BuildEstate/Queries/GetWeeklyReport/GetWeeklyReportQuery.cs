using GraphBridge.Application.Dtos.BuildEstate;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Queries.GetWeeklyReport;

/// <summary>
/// Query to retrieve the weekly report for a BuildEstate project.
/// Returns task counts by status, milestones due within 7 days, and team activity count.
/// </summary>
public class GetWeeklyReportQuery : IRequest<WeeklyReportDto>
{
    public Guid ProjectId { get; set; }
}
