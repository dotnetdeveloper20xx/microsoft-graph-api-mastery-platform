using GraphBridge.Application.Dtos.Onboarding;
using MediatR;

namespace GraphBridge.Application.Onboarding.Queries.GetOnboardingOverview;

/// <summary>
/// Handles retrieving all employee onboarding records as a list of DTOs.
/// </summary>
public class GetOnboardingOverviewQueryHandler : IRequestHandler<GetOnboardingOverviewQuery, IReadOnlyList<EmployeeOnboardingDto>>
{
    private readonly IEmployeeStore _employeeStore;

    public GetOnboardingOverviewQueryHandler(IEmployeeStore employeeStore)
    {
        _employeeStore = employeeStore;
    }

    public async Task<IReadOnlyList<EmployeeOnboardingDto>> Handle(GetOnboardingOverviewQuery request, CancellationToken cancellationToken)
    {
        var employees = await _employeeStore.GetAllAsync();

        return employees.Select(e => new EmployeeOnboardingDto
        {
            Id = e.Id,
            Name = e.Name,
            Role = e.Role,
            Department = e.Department,
            ManagerName = e.ManagerName,
            Email = e.Email,
            Status = new OnboardingStatusDto
            {
                ProfileCreated = e.ProfileCreated,
                GroupsAssigned = e.GroupsAssigned,
                WelcomeEmailSent = e.WelcomeEmailSent,
                InductionScheduled = e.InductionScheduled
            }
        }).ToList();
    }
}
