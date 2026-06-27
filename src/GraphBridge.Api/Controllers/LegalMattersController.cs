using GraphBridge.Application.Dtos.LegalMatters;
using GraphBridge.Application.LegalMatters.Commands.CreateMatter;
using GraphBridge.Application.LegalMatters.Commands.CreateWorkspace;
using GraphBridge.Application.LegalMatters.Commands.InviteParticipants;
using GraphBridge.Application.LegalMatters.Commands.ScheduleKickoff;
using GraphBridge.Application.LegalMatters.Queries.GetDocuments;
using GraphBridge.Application.LegalMatters.Queries.GetLegalMatterOverview;
using GraphBridge.Application.LegalMatters.Queries.GetMatterById;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Controller for Legal Matter Workspace Automation Module.
/// Provides endpoints for managing legal matters including workspace creation,
/// participant invitations, kickoff scheduling, and document retrieval.
/// </summary>
[Route("api/legal-matters")]
public class LegalMattersController : BaseApiController
{
    /// <summary>
    /// Retrieves all legal matters as an overview list.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<LegalMatterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await Mediator.Send(new GetLegalMatterOverviewQuery());
        var envelope = ApiEnvelope<IReadOnlyList<LegalMatterDto>>.Ok(result, "Legal matters overview retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Creates a new legal matter with a system-generated reference number.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiEnvelope<LegalMatterDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMatter([FromBody] CreateMatterCommand command)
    {
        var result = await Mediator.Send(command);
        var envelope = ApiEnvelope<LegalMatterDto>.Ok(result, "Legal matter created successfully");
        envelope.CorrelationId = GetCorrelationId();
        return StatusCode(StatusCodes.Status201Created, envelope);
    }

    /// <summary>
    /// Retrieves a single legal matter by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiEnvelope<LegalMatterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatter(Guid id)
    {
        var result = await Mediator.Send(new GetMatterByIdQuery { Id = id });
        var envelope = ApiEnvelope<LegalMatterDto>.Ok(result, "Legal matter retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Creates a Microsoft 365 workspace for the legal matter (SharePoint folders + Teams channel).
    /// </summary>
    [HttpPost("{id:guid}/create-workspace")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateWorkspace(Guid id)
    {
        await Mediator.Send(new CreateWorkspaceCommand { MatterId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Workspace created successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Invites participants to the legal matter workspace.
    /// </summary>
    [HttpPost("{id:guid}/invite-participants")]
    [ProducesResponseType(typeof(ApiEnvelope<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InviteParticipants(Guid id, [FromBody] InviteParticipantsRequest request)
    {
        var command = new InviteParticipantsCommand
        {
            MatterId = id,
            Participants = request.Participants
        };
        var count = await Mediator.Send(command);
        var envelope = ApiEnvelope<int>.Ok(count, $"{count} participants invited successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Schedules a kickoff meeting for the legal matter within 14 days.
    /// </summary>
    [HttpPost("{id:guid}/schedule-kickoff")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleKickoff(Guid id)
    {
        await Mediator.Send(new ScheduleKickoffCommand { MatterId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Kickoff meeting scheduled successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves the document folder tree structure for a legal matter workspace.
    /// </summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType(typeof(ApiEnvelope<MatterDocumentTreeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id)
    {
        var result = await Mediator.Send(new GetDocumentsQuery { MatterId = id });
        var envelope = ApiEnvelope<MatterDocumentTreeDto>.Ok(result, "Documents retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }
}
