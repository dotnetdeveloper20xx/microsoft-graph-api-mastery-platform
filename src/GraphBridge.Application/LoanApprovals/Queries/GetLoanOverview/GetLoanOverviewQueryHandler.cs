using GraphBridge.Application.Dtos.LoanApprovals;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Queries.GetLoanOverview;

/// <summary>
/// Handles retrieving all loan approval records as a list of DTOs.
/// </summary>
public class GetLoanOverviewQueryHandler : IRequestHandler<GetLoanOverviewQuery, IReadOnlyList<LoanApprovalDto>>
{
    private readonly ILoanApprovalStore _loanStore;

    public GetLoanOverviewQueryHandler(ILoanApprovalStore loanStore)
    {
        _loanStore = loanStore;
    }

    public async Task<IReadOnlyList<LoanApprovalDto>> Handle(GetLoanOverviewQuery request, CancellationToken cancellationToken)
    {
        var loans = await _loanStore.GetAllAsync();

        return loans.Select(loan => new LoanApprovalDto
        {
            Id = loan.Id,
            CustomerName = loan.CustomerName,
            Amount = loan.Amount,
            PropertyReference = loan.PropertyReference,
            Status = loan.Status,
            PackGenerated = loan.PackGenerated
        }).ToList();
    }
}
