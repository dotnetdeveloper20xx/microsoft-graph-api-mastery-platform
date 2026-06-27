using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetEmails;

/// <summary>
/// Handles retrieving recent emails from the past 24 hours, grouped by priority,
/// capped at 50 summaries.
/// </summary>
public class GetEmailsQueryHandler : IRequestHandler<GetEmailsQuery, IReadOnlyList<EmailSummaryDto>>
{
    private readonly IGraphMailService _mailService;

    public GetEmailsQueryHandler(IGraphMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task<IReadOnlyList<EmailSummaryDto>> Handle(GetEmailsQuery request, CancellationToken cancellationToken)
    {
        var emails = await _mailService.GetRecentEmailsAsync(24, cancellationToken);

        // Group by priority and return up to 50 summaries
        var grouped = emails
            .OrderByDescending(e => e.Priority == "high" ? 2 : e.Priority == "normal" ? 1 : 0)
            .ThenByDescending(e => e.ReceivedAt)
            .Take(50)
            .ToList();

        return grouped;
    }
}
