using GraphBridge.Application.Dtos.Graph;
using MediatR;

namespace GraphBridge.Application.Productivity.Queries.GetProductivityEmails;

/// <summary>
/// Query to retrieve email volume, top 10 senders, and unread count for the past 7 days.
/// </summary>
public class GetProductivityEmailsQuery : IRequest<EmailVolumeDto>
{
}
