using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphUserService for Demo_Mode.
/// Returns deterministic user profiles without any external HTTP or network calls.
/// </summary>
public class MockGraphUserService : IGraphUserService
{
    private static readonly List<UserProfileDto> _users = new()
    {
        new UserProfileDto
        {
            Id = "usr-001-sarah-khan",
            DisplayName = "Sarah Khan",
            Email = "sarah.khan@graphbridge.dev",
            Department = "HR",
            JobTitle = "HR Manager"
        },
        new UserProfileDto
        {
            Id = "usr-002-afzal-ahmed",
            DisplayName = "Afzal Ahmed",
            Email = "afzal.ahmed@graphbridge.dev",
            Department = "Finance",
            JobTitle = "Finance Director"
        },
        new UserProfileDto
        {
            Id = "usr-003-james-wilson",
            DisplayName = "James Wilson",
            Email = "james.wilson@graphbridge.dev",
            Department = "Legal",
            JobTitle = "Senior Solicitor"
        },
        new UserProfileDto
        {
            Id = "usr-004-emma-thompson",
            DisplayName = "Emma Thompson",
            Email = "emma.thompson@graphbridge.dev",
            Department = "IT",
            JobTitle = "IT Operations Lead"
        },
        new UserProfileDto
        {
            Id = "usr-005-david-chen",
            DisplayName = "David Chen",
            Email = "david.chen@graphbridge.dev",
            Department = "Operations",
            JobTitle = "Operations Manager"
        }
    };

    public Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);

        // Return matched user or default to Sarah Khan profile
        var result = user ?? new UserProfileDto
        {
            Id = userId,
            DisplayName = "Sarah Khan",
            Email = "sarah.khan@graphbridge.dev",
            Department = "HR",
            JobTitle = "HR Manager"
        };

        return Task.FromResult(result);
    }

    public Task<UserProfileDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var newUser = new UserProfileDto
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = request.DisplayName,
            Email = request.Email,
            Department = request.Department,
            JobTitle = request.JobTitle
        };

        return Task.FromResult(newUser);
    }

    public Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<UserProfileDto>>(_users.AsReadOnly());
    }
}
