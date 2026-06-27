using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Queries.GetLoanById;

/// <summary>
/// Handles retrieving a specific loan approval by its ID.
/// Throws NotFoundException if the loan does not exist.
/// </summary>
public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanApprovalDto>
{
    private readonly ILoanApprovalStore _loanStore;

    public GetLoanByIdQueryHandler(ILoanApprovalStore loanStore)
    {
        _loanStore = loanStore;
    }

    public async Task<LoanApprovalDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var loan = await _loanStore.GetByIdAsync(request.LoanId)
            ?? throw new NotFoundException("LoanApproval", request.LoanId);

        return new LoanApprovalDto
        {
            Id = loan.Id,
            CustomerName = loan.CustomerName,
            Amount = loan.Amount,
            PropertyReference = loan.PropertyReference,
            Status = loan.Status,
            PackGenerated = loan.PackGenerated
        };
    }
}
