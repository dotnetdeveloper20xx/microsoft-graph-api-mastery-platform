using FluentValidation;
using GraphBridge.Application.LegalMatters.Commands.CreateMatter;

namespace GraphBridge.Application.LegalMatters.Validators;

public class CreateMatterCommandValidator : AbstractValidator<CreateMatterCommand>
{
    public CreateMatterCommandValidator()
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
