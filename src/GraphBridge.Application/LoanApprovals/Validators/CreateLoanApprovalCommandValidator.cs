using FluentValidation;
using GraphBridge.Application.LoanApprovals.Commands.CreateLoanApproval;

namespace GraphBridge.Application.LoanApprovals.Validators;

public class CreateLoanApprovalCommandValidator : AbstractValidator<CreateLoanApprovalCommand>
{
    public CreateLoanApprovalCommandValidator()
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
