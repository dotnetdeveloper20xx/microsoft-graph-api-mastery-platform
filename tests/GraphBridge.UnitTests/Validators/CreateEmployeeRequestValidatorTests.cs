using FluentAssertions;
using FluentValidation.TestHelper;
using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Application.Onboarding.Validators;

namespace GraphBridge.UnitTests.Validators;

public class CreateEmployeeRequestValidatorTests
{
    private readonly CreateEmployeeRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            Name = "Sarah Khan",
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Afzal Ahmed",
            Email = "sarah.khan@company.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task EmptyName_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            Name = "",
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Afzal Ahmed",
            Email = "sarah.khan@company.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task NameExceeds100Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            Name = new string('A', 101),
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Afzal Ahmed",
            Email = "sarah.khan@company.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task InvalidEmail_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            Name = "Sarah Khan",
            Role = "Software Engineer",
            Department = "Engineering",
            ManagerName = "Afzal Ahmed",
            Email = "not-an-email"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task DepartmentExceeds50Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            Name = "Sarah Khan",
            Role = "Software Engineer",
            Department = new string('D', 51),
            ManagerName = "Afzal Ahmed",
            Email = "sarah.khan@company.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Department);
    }

    [Fact]
    public async Task AllFieldsEmpty_ShouldFailForAllFields()
    {
        // Arrange
        var request = new CreateEmployeeRequest();

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Role);
        result.ShouldHaveValidationErrorFor(x => x.Department);
        result.ShouldHaveValidationErrorFor(x => x.ManagerName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
