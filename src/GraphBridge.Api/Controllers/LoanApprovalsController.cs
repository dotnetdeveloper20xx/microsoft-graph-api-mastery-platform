using GraphBridge.Application.Dtos.LoanApprovals;
using GraphBridge.Application.LoanApprovals.Commands.CreateLoanApproval;
using GraphBridge.Application.LoanApprovals.Commands.GeneratePack;
using GraphBridge.Application.LoanApprovals.Commands.NotifyTeam;
using GraphBridge.Application.LoanApprovals.Commands.ScheduleFollowUp;
using GraphBridge.Application.LoanApprovals.Commands.SendCustomerEmail;
using GraphBridge.Application.LoanApprovals.Queries.GetAudit;
using GraphBridge.Application.LoanApprovals.Queries.GetLoanById;
using GraphBridge.Application.LoanApprovals.Queries.GetLoanOverview;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Controller for Loan Approval Communication Hub Module.
/// Provides endpoints for managing loan approvals including communication pack generation,
/// customer emails, team notifications, follow-up scheduling, and audit trail retrieval.
/// </summary>
[Route("api/loan-approvals")]
public class LoanApprovalsController : BaseApiController
{
    /// <summary>
    /// Retrieves all loan approval records as an overview.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<LoanApprovalDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await Mediator.Send(new GetLoanOverviewQuery());
        var envelope = ApiEnvelope<IReadOnlyList<LoanApprovalDto>>.Ok(result, "Loan approvals overview retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Creates a new loan approval record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiEnvelope<LoanApprovalDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLoanApproval([FromBody] CreateLoanApprovalCommand command)
    {
        var result = await Mediator.Send(command);
        var envelope = ApiEnvelope<LoanApprovalDto>.Ok(result, "Loan approval created successfully");
        envelope.CorrelationId = GetCorrelationId();
        return StatusCode(StatusCodes.Status201Created, envelope);
    }

    /// <summary>
    /// Retrieves a single loan approval by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiEnvelope<LoanApprovalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoanApproval(Guid id)
    {
        var result = await Mediator.Send(new GetLoanByIdQuery { LoanId = id });
        var envelope = ApiEnvelope<LoanApprovalDto>.Ok(result, "Loan approval retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Generates a communication pack for an approved loan.
    /// Only applicable to loans with "Approved" status.
    /// </summary>
    [HttpPost("{id:guid}/generate-pack")]
    [ProducesResponseType(typeof(ApiEnvelope<CommunicationPackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeneratePack(Guid id)
    {
        var result = await Mediator.Send(new GeneratePackCommand { LoanId = id });
        var envelope = ApiEnvelope<CommunicationPackDto>.Ok(result, "Communication pack generated successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Sends the approval notification email to the customer.
    /// Requires a communication pack to have been generated first.
    /// </summary>
    [HttpPost("{id:guid}/send-customer-email")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendCustomerEmail(Guid id)
    {
        await Mediator.Send(new SendCustomerEmailCommand { LoanId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Customer email sent successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Sends a Teams notification to the internal finance channel about the loan approval.
    /// </summary>
    [HttpPost("{id:guid}/notify-team")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NotifyTeam(Guid id)
    {
        await Mediator.Send(new NotifyTeamCommand { LoanId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Team notified successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Schedules a follow-up calendar event for the customer.
    /// </summary>
    [HttpPost("{id:guid}/schedule-follow-up")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleFollowUp(Guid id)
    {
        await Mediator.Send(new ScheduleFollowUpCommand { LoanId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Follow-up meeting scheduled successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves the audit trail for a specific loan approval.
    /// Returns entries in chronological order, limited to the most recent 100 entries.
    /// </summary>
    [HttpGet("{id:guid}/audit")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<AuditEntryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAudit(Guid id)
    {
        var result = await Mediator.Send(new GetAuditQuery { LoanId = id });
        var envelope = ApiEnvelope<IReadOnlyList<AuditEntryDto>>.Ok(result, "Audit trail retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }
}
