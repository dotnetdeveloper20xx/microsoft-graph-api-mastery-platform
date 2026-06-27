using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphTeamsService for Demo_Mode.
/// Returns channel creation results and notification confirmations.
/// No external HTTP or network calls are made.
/// </summary>
public class MockGraphTeamsService : IGraphTeamsService
{
    public Task<TeamChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken ct = default)
    {
        var channel = new TeamChannelDto
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = request.DisplayName,
            Description = request.Description
        };

        return Task.FromResult(channel);
    }

    public Task SendChannelNotificationAsync(SendChannelNotificationRequest request, CancellationToken ct = default)
    {
        // No-op in Demo_Mode — notifications are simulated as successful
        return Task.CompletedTask;
    }
}
