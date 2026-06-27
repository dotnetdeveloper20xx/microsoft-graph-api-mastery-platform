using FluentValidation;
using GraphBridge.Application.Dtos.LegalMatters;

namespace GraphBridge.Application.LegalMatters.Validators;

public class CreateLegalMatterRequestValidator : AbstractValidator<CreateLegalMatterRequest>
{
    public CreateLegalMatterRequestValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MatterType)
            .NotEmpty();

        RuleFor(x => x.AssignedSolicitor)
            .NotEmpty();
    }
}
