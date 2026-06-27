using GraphBridge.Application.Dtos.Productivity;
using MediatR;

namespace GraphBridge.Application.Productivity.Commands.GenerateWeeklySummary;

/// <summary>
/// Command to generate a weekly productivity summary aggregating calendar, emails,
/// tasks, and documents for the past 7 days.
/// </summary>
public class GenerateWeeklySummaryCommand : IRequest<ProductivitySummaryDto>
{
}
