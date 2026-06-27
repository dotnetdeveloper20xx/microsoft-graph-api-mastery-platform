using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.BuildEstate.Commands.NotifyDirectors;

/// <summary>
/// Handles sending notification emails to all assigned directors.
/// Checks at least 1 director is assigned, sends emails via IGraphMailService, returns count.
/// </summary>
public class NotifyDirectorsCommandHandler : IRequestHandler<NotifyDirectorsCommand, int>
{
    private readonly IBuildEstateProjectStore _projectStore;
    private readonly IGraphMailService _mailService;

    public NotifyDirectorsCommandHandler(IBuildEstateProjectStore projectStore, IGraphMailService mailService)
    {
        _projectStore = projectStore;
        _mailService = mailService;
    }

    public async Task<int> Handle(NotifyDirectorsCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectStore.GetByIdAsync(request.ProjectId)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (project.Directors.Count == 0)
        {
            throw new BusinessRuleException("At least one director must be assigned before notifications can be sent");
        }

        foreach (var director in project.Directors)
        {
            await _mailService.SendEmailAsync(new SendEmailRequest
            {
                To = director,
                Subject = $"Project Notification - {project.Name}",
                Body = $"Dear Director,\n\nYou have been assigned to the project '{project.Name}' located at {project.Location}.\n\nPlanning Status: {project.PlanningStatus}\n\nPlease review the project details at your earliest convenience.\n\nRegards,\nBuildEstate Project Management"
            }, cancellationToken);
        }

        return project.Directors.Count;
    }
}
