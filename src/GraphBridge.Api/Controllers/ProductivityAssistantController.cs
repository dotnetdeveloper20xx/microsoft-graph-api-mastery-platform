using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Dtos.Productivity;
using GraphBridge.Application.Productivity.Commands.GenerateWeeklySummary;
using GraphBridge.Application.Productivity.Queries.GetContextPackage;
using GraphBridge.Application.Productivity.Queries.GetProductivityCalendar;
using GraphBridge.Application.Productivity.Queries.GetProductivityDocuments;
using GraphBridge.Application.Productivity.Queries.GetProductivityEmails;
using GraphBridge.Application.Productivity.Queries.GetProductivityOverview;
using GraphBridge.Application.Productivity.Queries.GetProductivityTasks;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Controller for the AI Meeting and Productivity Assistant Module.
/// Provides endpoints for weekly productivity summaries, context packages,
/// and individual data sections aggregated from Microsoft 365 services.
/// </summary>
[Route("api/productivity-assistant")]
public class ProductivityAssistantController : BaseApiController
{
    /// <summary>
    /// Retrieves the productivity assistant overview with aggregated info.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiEnvelope<ProductivitySummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await Mediator.Send(new GetProductivityOverviewQuery());
        var envelope = ApiEnvelope<ProductivitySummaryDto>.Ok(result, "Productivity overview retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Generates a weekly productivity summary aggregating calendar, emails, tasks,
    /// and documents for the past 7 days.
    /// </summary>
    [HttpPost("weekly-summary")]
    [ProducesResponseType(typeof(ApiEnvelope<ProductivitySummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateWeeklySummary()
    {
        var result = await Mediator.Send(new GenerateWeeklySummaryCommand());
        var envelope = ApiEnvelope<ProductivitySummaryDto>.Ok(result, "Weekly summary generated successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves a structured AI context package containing sections for
    /// calendar, emails, tasks, and documents.
    /// </summary>
    [HttpGet("context-package")]
    [ProducesResponseType(typeof(ApiEnvelope<AiContextPackageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContextPackage()
    {
        var result = await Mediator.Send(new GetContextPackageQuery());
        var envelope = ApiEnvelope<AiContextPackageDto>.Ok(result, "Context package retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves calendar events for the current week (Monday to Sunday), max 100 events.
    /// </summary>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar()
    {
        var result = await Mediator.Send(new GetProductivityCalendarQuery());
        var envelope = ApiEnvelope<IReadOnlyList<CalendarEventDto>>.Ok(result, "Calendar events retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves email volume, top 10 senders, and unread count for the past 7 days.
    /// </summary>
    [HttpGet("emails")]
    [ProducesResponseType(typeof(ApiEnvelope<EmailVolumeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmails()
    {
        var result = await Mediator.Send(new GetProductivityEmailsQuery());
        var envelope = ApiEnvelope<EmailVolumeDto>.Ok(result, "Email summary retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves task completion summary (completed, overdue, in-progress) for the past 7 days.
    /// </summary>
    [HttpGet("tasks")]
    [ProducesResponseType(typeof(ApiEnvelope<TaskCompletionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks()
    {
        var result = await Mediator.Send(new GetProductivityTasksQuery());
        var envelope = ApiEnvelope<TaskCompletionSummaryDto>.Ok(result, "Task summary retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves documents accessed or modified within the past 7 days, max 50.
    /// </summary>
    [HttpGet("documents")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments()
    {
        var result = await Mediator.Send(new GetProductivityDocumentsQuery());
        var envelope = ApiEnvelope<IReadOnlyList<DocumentDto>>.Ok(result, "Documents retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }
}
