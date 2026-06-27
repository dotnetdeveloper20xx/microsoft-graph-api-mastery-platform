using GraphBridge.Application.Dtos.Onboarding;
using MediatR;

namespace GraphBridge.Application.Onboarding.Queries.GetOnboardingOverview;

/// <summary>
/// Query to retrieve all employee onboarding records.
/// </summary>
public class GetOnboardingOverviewQuery : IRequest<IReadOnlyList<EmployeeOnboardingDto>>
{
}
