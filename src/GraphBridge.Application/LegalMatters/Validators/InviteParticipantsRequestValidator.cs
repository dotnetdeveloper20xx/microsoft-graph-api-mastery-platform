using FluentValidation;
using GraphBridge.Application.Dtos.LegalMatters;

namespace GraphBridge.Application.LegalMatters.Validators;

public class InviteParticipantsRequestValidator : AbstractValidator<InviteParticipantsRequest>
{
    public InviteParticipantsRequestValidator()
    {
        RuleFor(x => x.Participants)
            .NotNull()
            .Must(list => list != null && list.Count >= 1 && list.Count <= 50)
            .WithMessage("Participants list must contain between 1 and 50 entries");
    }
}
