using FluentValidation;
using GraphBridge.Application.Dtos.LoanApprovals;

namespace GraphBridge.Application.LoanApprovals.Validators;

public class CreateLoanApprovalRequestValidator : AbstractValidator<CreateLoanApprovalRequest>
{
    public CreateLoanApprovalRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Amount)
            .InclusiveBetween(0.01m, 999999999.99m);

        RuleFor(x => x.PropertyReference)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .NotEmpty();
    }
}
