using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.CeoDashboard.Queries.GetEmails;

/// <summary>
/// Query to retrieve recent emails grouped by priority for the CEO dashboard,
/// limited to a maximum of 50 email summaries.
/// </summary>
public class GetEmailsQuery : IRequest<IReadOnlyList<EmailSummaryDto>>
{
}
