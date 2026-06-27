using GraphBridge.Application.BuildEstate.Commands.CreateProject;
using GraphBridge.Application.BuildEstate.Commands.CreateTaskBoard;
using GraphBridge.Application.BuildEstate.Commands.LaunchWorkspace;
using GraphBridge.Application.BuildEstate.Commands.NotifyDirectors;
using GraphBridge.Application.BuildEstate.Commands.ScheduleKickoff;
using GraphBridge.Application.BuildEstate.Queries.GetBuildEstateOverview;
using GraphBridge.Application.BuildEstate.Queries.GetProjectById;
using GraphBridge.Application.BuildEstate.Queries.GetWeeklyReport;
using GraphBridge.Application.Dtos.BuildEstate;
using GraphBridge.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GraphBridge.Api.Controllers;

/// <summary>
/// Controller for BuildEstate Project Launch Workspace Module.
/// Provides endpoints for managing BuildEstate projects including workspace launch,
/// task board creation, director notifications, kickoff scheduling, and weekly reports.
/// </summary>
[Route("api/buildestate-projects")]
public class BuildEstateProjectsController : BaseApiController
{
    /// <summary>
    /// Retrieves all BuildEstate projects as an overview.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ApiEnvelope<IReadOnlyList<BuildEstateProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var result = await Mediator.Send(new GetBuildEstateOverviewQuery());
        var envelope = ApiEnvelope<IReadOnlyList<BuildEstateProjectDto>>.Ok(result, "BuildEstate projects overview retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Creates a new BuildEstate project.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiEnvelope<BuildEstateProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        var result = await Mediator.Send(command);
        var envelope = ApiEnvelope<BuildEstateProjectDto>.Ok(result, "BuildEstate project created successfully");
        envelope.CorrelationId = GetCorrelationId();
        return StatusCode(StatusCodes.Status201Created, envelope);
    }

    /// <summary>
    /// Retrieves a single BuildEstate project by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiEnvelope<BuildEstateProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var result = await Mediator.Send(new GetProjectByIdQuery { ProjectId = id });
        var envelope = ApiEnvelope<BuildEstateProjectDto>.Ok(result, "BuildEstate project retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Launches a SharePoint workspace for the project with folder structure.
    /// Creates Planning Documents, Contracts, Site Reports, and Financial folders.
    /// </summary>
    [HttpPost("{id:guid}/launch-workspace")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LaunchWorkspace(Guid id)
    {
        await Mediator.Send(new LaunchWorkspaceCommand { ProjectId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Workspace launched successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Creates a Planner-style task board with default buckets and initial tasks.
    /// </summary>
    [HttpPost("{id:guid}/create-task-board")]
    [ProducesResponseType(typeof(ApiEnvelope<TaskBoardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTaskBoard(Guid id)
    {
        var result = await Mediator.Send(new CreateTaskBoardCommand { ProjectId = id });
        var envelope = ApiEnvelope<TaskBoardDto>.Ok(result, "Task board created successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Sends notification emails to all assigned directors of the project.
    /// </summary>
    [HttpPost("{id:guid}/notify-directors")]
    [ProducesResponseType(typeof(ApiEnvelope<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NotifyDirectors(Guid id)
    {
        var count = await Mediator.Send(new NotifyDirectorsCommand { ProjectId = id });
        var envelope = ApiEnvelope<int>.Ok(count, $"{count} directors notified successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Schedules a project kickoff calendar event for all team members within 14 days.
    /// </summary>
    [HttpPost("{id:guid}/schedule-kickoff")]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleKickoff(Guid id)
    {
        await Mediator.Send(new ScheduleKickoffCommand { ProjectId = id });
        var envelope = ApiEnvelope<object>.Ok(null!, "Kickoff meeting scheduled successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }

    /// <summary>
    /// Retrieves the weekly report for a BuildEstate project.
    /// Contains task counts by status, milestones due within 7 days, and team activity count.
    /// </summary>
    [HttpGet("{id:guid}/weekly-report")]
    [ProducesResponseType(typeof(ApiEnvelope<WeeklyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeeklyReport(Guid id)
    {
        var result = await Mediator.Send(new GetWeeklyReportQuery { ProjectId = id });
        var envelope = ApiEnvelope<WeeklyReportDto>.Ok(result, "Weekly report retrieved successfully");
        envelope.CorrelationId = GetCorrelationId();
        return Ok(envelope);
    }
}
