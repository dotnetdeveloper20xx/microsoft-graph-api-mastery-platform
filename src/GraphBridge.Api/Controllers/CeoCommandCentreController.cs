using GraphBridge.Application.CeoDashboard.Queries.GetCalendar;
using GraphBridge.Application.CeoDashboard.Queries.GetCeoOverview;
using GraphBridge.Application.CeoDashboard.Queries.GetSecuritySignals;
using GraphBridge.Application.CeoDashboard.Queries.GetToday;
using GraphBridge.Application.Dtos.CeoDashboard;
using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using CeoDashboardGetDocumentsQuery = GraphBridge.Application.CeoDashboard.Queries.GetDocuments.GetDocumentsQuery;
using CeoDashboardGetEmailsQuery = GraphBridge.Application.CeoDashboard.Queries.GetEmails.GetEmailsQuery;
using CeoDashboardGetTasksQuery = GraphBridge.Application.CeoDashboard.Queries.GetTasks.GetTasksQuery;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Controller for the CEO Command Centre Dashboard Module.
/// Provides read-only endpoints aggregating signals from Microsoft 365 services
/// including meetings, emails, tasks, documents, and security alerts.
/// </summary>
[Route("api/ceo-command-centre")]
public class CeoCommandCentreController : BaseApiController
{
    /// <summary>
    /// Retrieves the CEO command centre overview with aggregated counts
    /// from calendar, mail, planner, drive, and security services.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiEnvelope<CeoDashboardOverviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await Mediator.Send(new GetCeoOverviewQuery());
        var envelope = ApiEnvelope<CeoDashboardOverviewDto>.Ok(result, "CEO overview retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves today's calendar events for the CEO, limited to 50 events.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday()
    {
        var result = await Mediator.Send(new GetTodayQuery());
        var envelope = ApiEnvelope<IReadOnlyList<CalendarEventDto>>.Ok(result, "Today's events retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves recent emails grouped by priority, limited to 50 summaries.
    /// </summary>
    [HttpGet("emails")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<EmailSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmails()
    {
        var result = await Mediator.Send(new CeoDashboardGetEmailsQuery());
        var envelope = ApiEnvelope<IReadOnlyList<EmailSummaryDto>>.Ok(result, "Emails retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves today's calendar events using date range for the CEO dashboard.
    /// </summary>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar()
    {
        var result = await Mediator.Send(new GetCalendarQuery());
        var envelope = ApiEnvelope<IReadOnlyList<CalendarEventDto>>.Ok(result, "Calendar events retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves pending tasks for the CEO, limited to 50 tasks.
    /// </summary>
    [HttpGet("tasks")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<PlannerTaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks()
    {
        var result = await Mediator.Send(new CeoDashboardGetTasksQuery());
        var envelope = ApiEnvelope<IReadOnlyList<PlannerTaskDto>>.Ok(result, "Tasks retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves documents pending approval or recently modified, limited to 50 documents.
    /// </summary>
    [HttpGet("documents")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments()
    {
        var result = await Mediator.Send(new CeoDashboardGetDocumentsQuery());
        var envelope = ApiEnvelope<IReadOnlyList<DocumentDto>>.Ok(result, "Documents retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves security signals and alerts from the past 24 hours, limited to 50 signals.
    /// </summary>
    [HttpGet("security-signals")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<SecuritySignalDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecuritySignals()
    {
        var result = await Mediator.Send(new GetSecuritySignalsQuery());
        var envelope = ApiEnvelope<IReadOnlyList<SecuritySignalDto>>.Ok(result, "Security signals retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }
}
