using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.GeneratePack;

/// <summary>
/// Handles generation of a communication pack for an approved loan.
/// Validates the loan has "Approved" status, generates customer email content,
/// internal notification, and document checklist.
/// </summary>
public class GeneratePackCommandHandler : IRequestHandler<GeneratePackCommand, CommunicationPackDto>
{
    private readonly ILoanApprovalStore _loanStore;

    public GeneratePackCommandHandler(ILoanApprovalStore loanStore)
    {
        _loanStore = loanStore;
    }

    public async Task<CommunicationPackDto> Handle(GeneratePackCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanStore.GetByIdAsync(request.LoanId)
            ?? throw new NotFoundException("LoanApproval", request.LoanId);

        if (loan.Status != "Approved")
        {
            throw new BusinessRuleException("Communication packs can only be generated for approved loans");
        }

        var pack = new CommunicationPackDto
        {
            CustomerEmail = new EmailContentDto
            {
                Subject = "Loan Approval Confirmation",
                Body = $"Dear {loan.CustomerName},\n\nWe are pleased to confirm that your loan application for £{loan.Amount:N2} has been approved.\n\nProperty Reference: {loan.PropertyReference}\n\nPlease find the next steps below and do not hesitate to contact us if you have any questions.\n\nKind regards,\nGraphBridge Finance Team"
            },
            InternalNotificationContent = $"Loan approval communication pack generated for {loan.CustomerName} - Amount: £{loan.Amount:N2}, Property: {loan.PropertyReference}",
            DocumentChecklist = new List<string>
            {
                "Property valuation",
                "Proof of identity",
                "Income verification"
            }
        };

        loan.PackGenerated = true;
        await _loanStore.UpdateAsync(loan);

        return pack;
    }
}
