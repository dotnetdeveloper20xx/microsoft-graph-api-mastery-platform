using FluentAssertions;
using FluentValidation.TestHelper;
using GraphBridge.Application.BuildEstate.Validators;
using GraphBridge.Application.Dtos.BuildEstate;

namespace GraphBridge.UnitTests.Validators;

public class CreateBuildEstateProjectRequestValidatorTests
{
    private readonly CreateBuildEstateProjectRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateBuildEstateProjectRequest
        {
            Name = "Riverside Heights Development",
            Location = "Manchester, UK",
            PlanningStatus = "Approved",
            Directors = new List<string> { "director@company.com" }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task EmptyDirectorsList_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateBuildEstateProjectRequest
        {
            Name = "Riverside Heights Development",
            Location = "Manchester, UK",
            PlanningStatus = "Approved",
            Directors = new List<string>()
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Directors)
            .WithErrorMessage("At least one director must be assigned");
    }

    [Fact]
    public async Task NameExceeds200Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateBuildEstateProjectRequest
        {
            Name = new string('N', 201),
            Location = "Manchester, UK",
            PlanningStatus = "Approved",
            Directors = new List<string> { "director@company.com" }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task LocationExceeds200Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateBuildEstateProjectRequest
        {
            Name = "Riverside Heights Development",
            Location = new string('L', 201),
            PlanningStatus = "Approved",
            Directors = new List<string> { "director@company.com" }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public async Task EmptyPlanningStatus_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateBuildEstateProjectRequest
        {
            Name = "Riverside Heights Development",
            Location = "Manchester, UK",
            PlanningStatus = "",
            Directors = new List<string> { "director@company.com" }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlanningStatus);
    }

    [Fact]
    public async Task MultipleDirectors_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateBuildEstateProjectRequest
        {
            Name = "Riverside Heights Development",
            Location = "Manchester, UK",
            PlanningStatus = "Approved",
            Directors = new List<string>
            {
                "director1@company.com",
                "director2@company.com",
                "director3@company.com"
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
