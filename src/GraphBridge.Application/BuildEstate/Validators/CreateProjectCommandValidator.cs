using FluentValidation;
using GraphBridge.Application.BuildEstate.Commands.CreateProject;

namespace GraphBridge.Application.BuildEstate.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PlanningStatus)
            .NotEmpty();

        RuleFor(x => x.Directors)
            .NotNull()
            .Must(list => list != null && list.Count >= 1)
            .WithMessage("At least one director must be assigned");
    }
}
