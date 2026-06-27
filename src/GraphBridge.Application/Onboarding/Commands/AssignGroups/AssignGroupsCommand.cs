using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.AssignGroups;

/// <summary>
/// Command to assign Microsoft 365 groups to an employee based on their department.
/// </summary>
public class AssignGroupsCommand : IRequest<Unit>
{
    public Guid EmployeeId { get; set; }
}
