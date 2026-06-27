using FluentAssertions;
using FluentValidation.TestHelper;
using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Application.LoanApprovals.Validators;

namespace GraphBridge.UnitTests.Validators;

public class CreateLoanApprovalRequestValidatorTests
{
    private readonly CreateLoanApprovalRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 250000.00m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task AmountBelowMinimum_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 0.00m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public async Task AmountAboveMaximum_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 1000000000.00m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public async Task EmptyCustomerName_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "",
            Amount = 250000.00m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public async Task CustomerNameExceeds200Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = new string('N', 201),
            Amount = 250000.00m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public async Task PropertyReferenceExceeds100Characters_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 250000.00m,
            PropertyReference = new string('P', 101),
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PropertyReference);
    }

    [Fact]
    public async Task EmptyStatus_ShouldFailWithError()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 250000.00m,
            PropertyReference = "PROP-2024-001",
            Status = ""
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public async Task MinimumValidAmount_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 0.01m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task MaximumValidAmount_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "Greenway Property Holdings",
            Amount = 999999999.99m,
            PropertyReference = "PROP-2024-001",
            Status = "Approved"
        };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
