using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityEmails;

/// <summary>
/// Handles retrieving email volume, top 10 senders, and unread count for the past 7 days.
/// </summary>
public class GetProductivityEmailsQueryHandler : IRequestHandler<GetProductivityEmailsQuery, EmailVolumeDto>
{
    private readonly IGraphMailService _mailService;

    public GetProductivityEmailsQueryHandler(IGraphMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task<EmailVolumeDto> Handle(GetProductivityEmailsQuery request, CancellationToken cancellationToken)
    {
        return await _mailService.GetEmailVolumeAsync(7, cancellationToken);
    }
}
