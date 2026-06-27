using FluentAssertions;
using FluentValidation.TestHelper;
using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Application.LegalMatters.Validators;

namespace GraphBridge.UnitTests.Validators;

public class InviteParticipantsRequestValidatorTests
{
    private readonly InviteParticipantsRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_OneParticipant_ShouldPassValidation()
    {
        // Arrange
        var request = new InviteParticipantsRequest
        {
            Participants = new List<string> { "participant@law.co.uk" }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidRequest_FiftyParticipants_ShouldPassValidation()
    {
        // Arrange
        var request = new InviteParticipantsRequest
        {
            Participants = Enumerable.Range(1, 50)
                .Select(i => $"participant{i}@law.co.uk")
                .ToList()
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task EmptyParticipantsList_ShouldFailWithError()
    {
        // Arrange
        var request = new InviteParticipantsRequest
        {
            Participants = new List<string>()
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Participants)
            .WithErrorMessage("Participants list must contain between 1 and 50 entries");
    }

    [Fact]
    public async Task MoreThan50Participants_ShouldFailWithError()
    {
        // Arrange
        var request = new InviteParticipantsRequest
        {
            Participants = Enumerable.Range(1, 51)
                .Select(i => $"participant{i}@law.co.uk")
                .ToList()
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Participants)
            .WithErrorMessage("Participants list must contain between 1 and 50 entries");
    }

    [Fact]
    public async Task NullParticipantsList_ShouldFailWithError()
    {
        // Arrange
        var request = new InviteParticipantsRequest
        {
            Participants = null!
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Participants);
    }
}
