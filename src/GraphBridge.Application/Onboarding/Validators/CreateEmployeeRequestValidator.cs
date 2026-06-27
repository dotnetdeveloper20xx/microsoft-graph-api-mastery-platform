using FluentValidation;
using GraphBridge.Application.Dtos.Onboarding;

namespace GraphBridge.Application.Onboarding.Validators;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(100);

        RuleFor(x => x.Department)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50);

        RuleFor(x => x.ManagerName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
