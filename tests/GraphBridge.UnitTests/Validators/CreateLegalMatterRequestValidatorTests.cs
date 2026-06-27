using FluentAssertions;
using FluentValidation.TestHelper;
using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Application.LegalMatters.Validators;

namespace GraphBridge.UnitTests.Validators;

public class CreateLegalMatterRequestValidatorTests
{
    private readonly CreateLegalMatterRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateLegalMatterRequest
        {
            ClientName = "Oakfield Estates Ltd",
            MatterType = "Commercial Property",
            AssignedSolicitor = "James Richardson"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task EmptyClientName_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLegalMatterRequest
        {
            ClientName = "",
            MatterType = "Commercial Property",
            AssignedSolicitor = "James Richardson"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientName);
    }

    [Fact]
    public async Task ClientNameExceeds200Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLegalMatterRequest
        {
            ClientName = new string('C', 201),
            MatterType = "Commercial Property",
            AssignedSolicitor = "James Richardson"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientName);
    }

    [Fact]
    public async Task EmptyMatterType_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLegalMatterRequest
        {
            ClientName = "Oakfield Estates Ltd",
            MatterType = "",
            AssignedSolicitor = "James Richardson"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MatterType);
    }

    [Fact]
    public async Task EmptyAssignedSolicitor_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLegalMatterRequest
        {
            ClientName = "Oakfield Estates Ltd",
            MatterType = "Commercial Property",
            AssignedSolicitor = ""
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AssignedSolicitor);
    }

    [Fact]
    public async Task AllFieldsEmpty_ShouldFailForAllFields()
    {
        // Arrange
        var request = new CreateLegalMatterRequest();

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientName);
        result.ShouldHaveValidationErrorFor(x => x.MatterType);
        result.ShouldHaveValidationErrorFor(x => x.AssignedSolicitor);
    }
}
