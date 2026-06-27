using FluentAssertions;
using FluentValidation;
using FsCheck;
using FsCheck.Xunit;
using GraphBridge.Application.Behaviors;
using GraphBridge.Application.Dtos.BuildEstate;
using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Application.BuildEstate.Validators;
using GraphBridge.Application.LegalMatters.Validators;
using GraphBridge.Application.LoanApprovals.Validators;
using GraphBridge.Application.Onboarding.Validators;
using MediatR;
using Moq;

namespace GraphBridge.UnitTests.PropertyTests;

/// <summary>
/// Property 3: Validation Rejection Prevents Handler Execution
/// For any command request that violates one or more FluentValidation rules,
/// the MediatR pipeline SHALL return a validation failure without invoking the
/// corresponding CQRS handler.
/// 
/// **Validates: Requirements 1.7, 2.3, 5.2, 6.6**
/// </summary>
public class ValidationRejectionPreventsHandlerExecutionPropertyTests
{
    #region Generators

    /// <summary>
    /// Generates CreateEmployeeRequest instances that are invalid because of
    /// empty/null Name OR Email without @.
    /// </summary>
    private static Arbitrary<CreateEmployeeRequest> InvalidEmployeeRequestArbitrary()
    {
        var invalidNameGen = from role in Arb.Generate<NonEmptyString>()
                             from dept in Gen.Elements("HR", "Finance", "Engineering", "Sales")
                             from manager in Arb.Generate<NonEmptyString>()
                             from email in Gen.Elements("test@example.com", "valid@domain.co.uk")
                             from nameChoice in Gen.Elements("", null as string)
                             select new CreateEmployeeRequest
                             {
                                 Name = nameChoice ?? string.Empty,
                                 Role = role.Get.Substring(0, Math.Min(role.Get.Length, 100)),
                                 Department = dept,
                                 ManagerName = manager.Get.Substring(0, Math.Min(manager.Get.Length, 100)),
                                 Email = email
                             };

        var invalidEmailGen = from name in Arb.Generate<NonEmptyString>()
                              from role in Arb.Generate<NonEmptyString>()
                              from dept in Gen.Elements("HR", "Finance", "Engineering", "Sales")
                              from manager in Arb.Generate<NonEmptyString>()
                              from email in Gen.Elements("invalid-no-at", "justastring", "noatsign.com", "", "spaces only")
                              select new CreateEmployeeRequest
                              {
                                  Name = name.Get.Substring(0, Math.Min(name.Get.Length, 100)),
                                  Role = role.Get.Substring(0, Math.Min(role.Get.Length, 100)),
                                  Department = dept,
                                  ManagerName = manager.Get.Substring(0, Math.Min(manager.Get.Length, 100)),
                                  Email = email
                              };

        var combined = Gen.OneOf(invalidNameGen, invalidEmailGen);
        return combined.ToArbitrary();
    }

    /// <summary>
    /// Generates CreateLegalMatterRequest instances with empty MatterType.
    /// </summary>
    private static Arbitrary<CreateLegalMatterRequest> InvalidLegalMatterRequestArbitrary()
    {
        var gen = from clientName in Arb.Generate<NonEmptyString>()
                  from solicitor in Arb.Generate<NonEmptyString>()
                  from matterType in Gen.Elements("", null as string)
                  select new CreateLegalMatterRequest
                  {
                      ClientName = clientName.Get.Substring(0, Math.Min(clientName.Get.Length, 200)),
                      MatterType = matterType ?? string.Empty,
                      AssignedSolicitor = solicitor.Get.Substring(0, Math.Min(solicitor.Get.Length, 100))
                  };

        return gen.ToArbitrary();
    }

    /// <summary>
    /// Generates CreateLoanApprovalRequest instances with Amount ≤ 0 or Amount > 999999999.99.
    /// </summary>
    private static Arbitrary<CreateLoanApprovalRequest> InvalidLoanApprovalRequestArbitrary()
    {
        var negativeOrZeroGen = from customerName in Arb.Generate<NonEmptyString>()
                                from propRef in Arb.Generate<NonEmptyString>()
                                from status in Gen.Elements("Approved", "Pending", "Rejected")
                                from amount in Gen.OneOf(
                                    Gen.Choose(-1000000, 0).Select(x => (decimal)x),
                                    Gen.Constant(0m),
                                    Gen.Constant(-0.01m),
                                    Gen.Constant(-999.99m))
                                select new CreateLoanApprovalRequest
                                {
                                    CustomerName = customerName.Get.Substring(0, Math.Min(customerName.Get.Length, 200)),
                                    Amount = amount,
                                    PropertyReference = propRef.Get.Substring(0, Math.Min(propRef.Get.Length, 100)),
                                    Status = status
                                };

        var tooHighGen = from customerName in Arb.Generate<NonEmptyString>()
                         from propRef in Arb.Generate<NonEmptyString>()
                         from status in Gen.Elements("Approved", "Pending", "Rejected")
                         from excess in Gen.Choose(1, 100000).Select(x => (decimal)x)
                         select new CreateLoanApprovalRequest
                         {
                             CustomerName = customerName.Get.Substring(0, Math.Min(customerName.Get.Length, 200)),
                             Amount = 999999999.99m + excess,
                             PropertyReference = propRef.Get.Substring(0, Math.Min(propRef.Get.Length, 100)),
                             Status = status
                         };

        var combined = Gen.OneOf(negativeOrZeroGen, tooHighGen);
        return combined.ToArbitrary();
    }

    /// <summary>
    /// Generates CreateBuildEstateProjectRequest instances with empty Directors list.
    /// </summary>
    private static Arbitrary<CreateBuildEstateProjectRequest> InvalidBuildEstateRequestArbitrary()
    {
        var gen = from name in Arb.Generate<NonEmptyString>()
                  from location in Arb.Generate<NonEmptyString>()
                  from planningStatus in Gen.Elements("Approved", "Pending", "Submitted")
                  from directors in Gen.Elements(new List<string>(), null as List<string>)
                  select new CreateBuildEstateProjectRequest
                  {
                      Name = name.Get.Substring(0, Math.Min(name.Get.Length, 200)),
                      Location = location.Get.Substring(0, Math.Min(location.Get.Length, 200)),
                      PlanningStatus = planningStatus,
                      Directors = directors ?? new List<string>()
                  };

        return gen.ToArbitrary();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Executes the ValidationBehavior pipeline with the given request and validators,
    /// tracking whether the handler (next delegate) is ever invoked.
    /// </summary>
    private static async Task<(bool HandlerInvoked, ValidationException? Exception)> ExecuteValidationPipeline<TRequest, TResponse>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators)
        where TRequest : notnull
    {
        var behavior = new ValidationBehavior<TRequest, TResponse>(validators);
        var handlerInvoked = false;

        RequestHandlerDelegate<TResponse> next = () =>
        {
            handlerInvoked = true;
            return Task.FromResult(default(TResponse)!);
        };

        ValidationException? caughtException = null;
        try
        {
            await behavior.Handle(request, next, CancellationToken.None);
        }
        catch (ValidationException ex)
        {
            caughtException = ex;
        }

        return (handlerInvoked, caughtException);
    }

    #endregion

    /// <summary>
    /// For any CreateEmployeeRequest with empty/null Name OR Email without @ →
    /// validation fails and the handler is never called.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidEmployeeRequest_ValidationFails_HandlerNeverCalled()
    {
        return Prop.ForAll(
            InvalidEmployeeRequestArbitrary(),
            request =>
            {
                var validators = new List<IValidator<CreateEmployeeRequest>>
                {
                    new CreateEmployeeRequestValidator()
                };

                var (handlerInvoked, exception) = ExecuteValidationPipeline<CreateEmployeeRequest, EmployeeOnboardingDto>(
                    request, validators).GetAwaiter().GetResult();

                // Validation must throw
                exception.Should().NotBeNull(
                    "invalid employee request should cause ValidationException to be thrown");

                // Handler must never be invoked
                handlerInvoked.Should().BeFalse(
                    "handler should never be invoked when validation fails");

                // Exception should contain at least one validation error
                exception!.Errors.Should().NotBeEmpty(
                    "validation exception should contain at least one error");
            });
    }

    /// <summary>
    /// For any CreateLegalMatterRequest with empty MatterType →
    /// validation fails and the handler is never called.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidLegalMatterRequest_ValidationFails_HandlerNeverCalled()
    {
        return Prop.ForAll(
            InvalidLegalMatterRequestArbitrary(),
            request =>
            {
                var validators = new List<IValidator<CreateLegalMatterRequest>>
                {
                    new CreateLegalMatterRequestValidator()
                };

                var (handlerInvoked, exception) = ExecuteValidationPipeline<CreateLegalMatterRequest, LegalMatterDto>(
                    request, validators).GetAwaiter().GetResult();

                // Validation must throw
                exception.Should().NotBeNull(
                    "invalid legal matter request (empty MatterType) should cause ValidationException");

                // Handler must never be invoked
                handlerInvoked.Should().BeFalse(
                    "handler should never be invoked when validation fails");

                // Exception should reference the MatterType field
                exception!.Errors.Should().Contain(e => e.PropertyName == "MatterType",
                    "validation errors should include MatterType violation");
            });
    }

    /// <summary>
    /// For any CreateLoanApprovalRequest with Amount ≤ 0 or Amount > 999999999.99 →
    /// validation fails and the handler is never called.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidLoanApprovalRequest_ValidationFails_HandlerNeverCalled()
    {
        return Prop.ForAll(
            InvalidLoanApprovalRequestArbitrary(),
            request =>
            {
                var validators = new List<IValidator<CreateLoanApprovalRequest>>
                {
                    new CreateLoanApprovalRequestValidator()
                };

                var (handlerInvoked, exception) = ExecuteValidationPipeline<CreateLoanApprovalRequest, LoanApprovalDto>(
                    request, validators).GetAwaiter().GetResult();

                // Validation must throw
                exception.Should().NotBeNull(
                    $"loan request with Amount={request.Amount} should cause ValidationException");

                // Handler must never be invoked
                handlerInvoked.Should().BeFalse(
                    "handler should never be invoked when validation fails");

                // Exception should reference the Amount field
                exception!.Errors.Should().Contain(e => e.PropertyName == "Amount",
                    $"validation errors should include Amount violation for Amount={request.Amount}");
            });
    }

    /// <summary>
    /// For any CreateBuildEstateProjectRequest with empty Directors list →
    /// validation fails and the handler is never called.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidBuildEstateRequest_ValidationFails_HandlerNeverCalled()
    {
        return Prop.ForAll(
            InvalidBuildEstateRequestArbitrary(),
            request =>
            {
                var validators = new List<IValidator<CreateBuildEstateProjectRequest>>
                {
                    new CreateBuildEstateProjectRequestValidator()
                };

                var (handlerInvoked, exception) = ExecuteValidationPipeline<CreateBuildEstateProjectRequest, BuildEstateProjectDto>(
                    request, validators).GetAwaiter().GetResult();

                // Validation must throw
                exception.Should().NotBeNull(
                    "build estate request with empty directors should cause ValidationException");

                // Handler must never be invoked
                handlerInvoked.Should().BeFalse(
                    "handler should never be invoked when validation fails");

                // Exception should reference the Directors field
                exception!.Errors.Should().Contain(e => e.PropertyName == "Directors",
                    "validation errors should include Directors violation");
            });
    }

    /// <summary>
    /// Verifies with Moq that a mock handler delegate is never invoked when
    /// validation rejects the request, using various invalid request types.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MockHandler_NeverInvoked_WhenValidationRejects()
    {
        // Generate a random choice of which validator to test
        return Prop.ForAll(
            Gen.Choose(1, 4).ToArbitrary(),
            choice =>
            {
                switch (choice)
                {
                    case 1:
                        VerifyMockHandlerNotInvokedForEmployee();
                        break;
                    case 2:
                        VerifyMockHandlerNotInvokedForLegalMatter();
                        break;
                    case 3:
                        VerifyMockHandlerNotInvokedForLoanApproval();
                        break;
                    case 4:
                        VerifyMockHandlerNotInvokedForBuildEstate();
                        break;
                }
            });
    }

    private void VerifyMockHandlerNotInvokedForEmployee()
    {
        var request = new CreateEmployeeRequest
        {
            Name = "", // Invalid: empty name
            Role = "Developer",
            Department = "Engineering",
            ManagerName = "John Smith",
            Email = "test@example.com"
        };

        var mockNext = new Mock<RequestHandlerDelegate<EmployeeOnboardingDto>>();
        var behavior = new ValidationBehavior<CreateEmployeeRequest, EmployeeOnboardingDto>(
            new[] { new CreateEmployeeRequestValidator() });

        var act = () => behavior.Handle(request, mockNext.Object, CancellationToken.None);
        act.Should().ThrowAsync<ValidationException>().GetAwaiter().GetResult();

        mockNext.Verify(x => x(), Times.Never(),
            "Handler delegate must never be invoked when validation fails");
    }

    private void VerifyMockHandlerNotInvokedForLegalMatter()
    {
        var request = new CreateLegalMatterRequest
        {
            ClientName = "Test Client",
            MatterType = "", // Invalid: empty
            AssignedSolicitor = "Jane Doe"
        };

        var mockNext = new Mock<RequestHandlerDelegate<LegalMatterDto>>();
        var behavior = new ValidationBehavior<CreateLegalMatterRequest, LegalMatterDto>(
            new[] { new CreateLegalMatterRequestValidator() });

        var act = () => behavior.Handle(request, mockNext.Object, CancellationToken.None);
        act.Should().ThrowAsync<ValidationException>().GetAwaiter().GetResult();

        mockNext.Verify(x => x(), Times.Never(),
            "Handler delegate must never be invoked when validation fails");
    }

    private void VerifyMockHandlerNotInvokedForLoanApproval()
    {
        var request = new CreateLoanApprovalRequest
        {
            CustomerName = "John Doe",
            Amount = 0m, // Invalid: must be >= 0.01
            PropertyReference = "PROP-001",
            Status = "Pending"
        };

        var mockNext = new Mock<RequestHandlerDelegate<LoanApprovalDto>>();
        var behavior = new ValidationBehavior<CreateLoanApprovalRequest, LoanApprovalDto>(
            new[] { new CreateLoanApprovalRequestValidator() });

        var act = () => behavior.Handle(request, mockNext.Object, CancellationToken.None);
        act.Should().ThrowAsync<ValidationException>().GetAwaiter().GetResult();

        mockNext.Verify(x => x(), Times.Never(),
            "Handler delegate must never be invoked when validation fails");
    }

    private void VerifyMockHandlerNotInvokedForBuildEstate()
    {
        var request = new CreateBuildEstateProjectRequest
        {
            Name = "Riverside Heights",
            Location = "London",
            PlanningStatus = "Approved",
            Directors = new List<string>() // Invalid: empty list
        };

        var mockNext = new Mock<RequestHandlerDelegate<BuildEstateProjectDto>>();
        var behavior = new ValidationBehavior<CreateBuildEstateProjectRequest, BuildEstateProjectDto>(
            new[] { new CreateBuildEstateProjectRequestValidator() });

        var act = () => behavior.Handle(request, mockNext.Object, CancellationToken.None);
        act.Should().ThrowAsync<ValidationException>().GetAwaiter().GetResult();

        mockNext.Verify(x => x(), Times.Never(),
            "Handler delegate must never be invoked when validation fails");
    }
}
