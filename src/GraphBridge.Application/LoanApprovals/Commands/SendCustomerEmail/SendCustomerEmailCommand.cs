using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.SendCustomerEmail;

/// <summary>
/// Command to send the approval notification email to the customer.
/// </summary>
public class SendCustomerEmailCommand : IRequest<Unit>
{
    public Guid LoanId { get; set; }
}
