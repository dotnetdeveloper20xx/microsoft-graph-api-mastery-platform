using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Domain.Entities;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.CreateLoanApproval;

/// <summary>
/// Handles creation of a new loan approval record.
/// Generates a unique GUID, persists the entity, and returns the DTO.
/// </summary>
public class CreateLoanApprovalCommandHandler : IRequestHandler<CreateLoanApprovalCommand, LoanApprovalDto>
{
    private readonly ILoanApprovalStore _loanStore;

    public CreateLoanApprovalCommandHandler(ILoanApprovalStore loanStore)
    {
        _loanStore = loanStore;
    }

    public async Task<LoanApprovalDto> Handle(CreateLoanApprovalCommand request, CancellationToken cancellationToken)
    {
        var loan = new LoanApproval
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            Amount = request.Amount,
            PropertyReference = request.PropertyReference,
            Status = request.Status,
            PackGenerated = false,
            AuditEntries = new List<LoanAuditEntry>()
        };

        await _loanStore.AddAsync(loan);

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
