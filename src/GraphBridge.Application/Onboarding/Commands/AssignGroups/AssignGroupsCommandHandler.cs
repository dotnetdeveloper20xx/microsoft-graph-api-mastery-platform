using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.AssignGroups;

/// <summary>
/// Handles assigning Microsoft 365 groups to an employee based on their department.
/// Calls IGraphGroupService to determine and assign appropriate groups.
/// </summary>
public class AssignGroupsCommandHandler : IRequestHandler<AssignGroupsCommand, Unit>
{
    private readonly IEmployeeStore _employeeStore;
    private readonly IGraphGroupService _graphGroupService;

    public AssignGroupsCommandHandler(IEmployeeStore employeeStore, IGraphGroupService graphGroupService)
    {
        _employeeStore = employeeStore;
        _graphGroupService = graphGroupService;
    }

    public async Task<Unit> Handle(AssignGroupsCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeStore.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        var groups = await _graphGroupService.GetGroupsForDepartmentAsync(employee.Department, cancellationToken);
        var groupIds = groups.Select(g => g.Id).ToList();

        await _graphGroupService.AssignUserToGroupsAsync(employee.Id.ToString(), groupIds, cancellationToken);

        employee.GroupsAssigned = true;
        await _employeeStore.UpdateAsync(employee);

        return Unit.Value;
    }
}
