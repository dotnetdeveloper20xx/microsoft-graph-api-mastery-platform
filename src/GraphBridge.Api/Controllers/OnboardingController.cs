using GraphBridge.Application.Dtos.Onboarding;
using GraphBridge.Application.Onboarding.Commands.AssignGroups;
using GraphBridge.Application.Onboarding.Commands.CreateEmployee;
using GraphBridge.Application.Onboarding.Commands.ScheduleInduction;
using GraphBridge.Application.Onboarding.Commands.SendWelcomeEmail;
using GraphBridge.Application.Onboarding.Queries.GetEmployeeById;
using GraphBridge.Application.Onboarding.Queries.GetEmployeeStatus;
using GraphBridge.Application.Onboarding.Queries.GetOnboardingOverview;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Controller for Employee Onboarding Automation Module.
/// Provides endpoints for managing employee onboarding workflows including
/// group assignment, welcome emails, and induction scheduling.
/// </summary>
[Route("api/onboarding")]
public class OnboardingController : BaseApiController
{
    /// <summary>
    /// Retrieves all employee onboarding records as an overview.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<EmployeeOnboardingDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await Mediator.Send(new GetOnboardingOverviewQuery());
        var envelope = ApiEnvelope<IReadOnlyList<EmployeeOnboardingDto>>.Ok(result, "Onboarding overview retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Creates a new employee onboarding record.
    /// </summary>
    [HttpPost("employees")]
    [ProducesResponseType(typeof(ApiEnvelope<EmployeeOnboardingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
    {
        var result = await Mediator.Send(command);
        var envelope = ApiEnvelope<EmployeeOnboardingDto>.Ok(result, "Employee created successfully");
        envelope.CorrelationId = GetCorrelationId();
        return StatusCode(StatusCodes.Status201Created, envelope);
    }

    /// <summary>
    /// Retrieves a single employee onboarding record by ID.
    /// </summary>
    [HttpGet("employees/{id:guid}")]
    [ProducesResponseType(typeof(ApiEnvelope<EmployeeOnboardingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery { EmployeeId = id });
        var envelope = ApiEnvelope<EmployeeOnboardingDto>.Ok(result, "Employee retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Assigns Microsoft 365 groups to an employee based on their department.
    /// </summary>
    [HttpPost("employees/{id:guid}/assign-groups")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignGroups(Guid id)
    {
        await Mediator.Send(new AssignGroupsCommand { EmployeeId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Groups assigned successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Sends a welcome email to the employee.
    /// </summary>
    [HttpPost("employees/{id:guid}/send-welcome-email")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendWelcomeEmail(Guid id)
    {
        await Mediator.Send(new SendWelcomeEmailCommand { EmployeeId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Welcome email sent successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Schedules an induction meeting for the employee and their manager.
    /// </summary>
    [HttpPost("employees/{id:guid}/schedule-induction")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleInduction(Guid id)
    {
        await Mediator.Send(new ScheduleInductionCommand { EmployeeId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Induction meeting scheduled successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves the onboarding status for a specific employee.
    /// </summary>
    [HttpGet("employees/{id:guid}/status")]
    [ProducesResponseType(typeof(ApiEnvelope<OnboardingStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeStatusQuery { EmployeeId = id });
        var envelope = ApiEnvelope<OnboardingStatusDto>.Ok(result, "Employee status retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }
}
