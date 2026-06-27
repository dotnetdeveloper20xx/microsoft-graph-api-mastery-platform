using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Domain.Entities;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.SendCustomerEmail;

/// <summary>
/// Handles sending the approval notification email to the customer.
/// Validates that the communication pack has been generated first,
/// calls IGraphMailService, and creates an audit trail entry.
/// </summary>
public class SendCustomerEmailCommandHandler : IRequestHandler<SendCustomerEmailCommand, Unit>
{
    private readonly ILoanApprovalStore _loanStore;
    private readonly IGraphMailService _mailService;

    public SendCustomerEmailCommandHandler(ILoanApprovalStore loanStore, IGraphMailService mailService)
    {
        _loanStore = loanStore;
        _mailService = mailService;
    }

    public async Task<Unit> Handle(SendCustomerEmailCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanStore.GetByIdAsync(request.LoanId)
            ?? throw new NotFoundException("LoanApproval", request.LoanId);

        if (!loan.PackGenerated)
        {
            throw new BusinessRuleException("Communication pack must be generated first");
        }

        await _mailService.SendEmailAsync(new SendEmailRequest
        {
            To = $"{loan.CustomerName.ToLower().Replace(" ", ".")}@customer.example.com",
            Subject = "Loan Approval Confirmation",
            Body = $"Dear {loan.CustomerName},\n\nYour loan application for £{loan.Amount:N2} has been approved.\n\nProperty Reference: {loan.PropertyReference}\n\nKind regards,\nGraphBridge Finance Team"
        }, cancellationToken);

        var auditEntry = new LoanAuditEntry
        {
            ActionType = "CustomerEmailSent",
            Timestamp = DateTime.UtcNow,
            Status = "Completed"
        };

        loan.AuditEntries.Add(auditEntry);
        await _loanStore.UpdateAsync(loan);

        return Unit.Value;
    }
}
