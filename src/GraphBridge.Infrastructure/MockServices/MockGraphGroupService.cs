using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;

namespace GraphBridge.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of IGraphGroupService for Demo_Mode.
/// Returns department-based groups without any external HTTP or network calls.
/// </summary>
public class MockGraphGroupService : IGraphGroupService
{
    private static readonly Dictionary<string, List<GroupDto>> _departmentGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HR"] = new List<GroupDto>
        {
            new GroupDto { Id = "grp-hr-001", DisplayName = "HR Team", Description = "Human Resources department team for workforce management" },
            new GroupDto { Id = "grp-hr-002", DisplayName = "People & Culture", Description = "People and culture committee for employee engagement" }
        },
        ["Finance"] = new List<GroupDto>
        {
            new GroupDto { Id = "grp-fin-001", DisplayName = "Finance Team", Description = "Finance department team for financial operations" },
            new GroupDto { Id = "grp-fin-002", DisplayName = "Budget Committee", Description = "Budget planning and oversight committee" }
        },
        ["Legal"] = new List<GroupDto>
        {
            new GroupDto { Id = "grp-leg-001", DisplayName = "Legal Team", Description = "Legal department team for compliance and advisory" },
            new GroupDto { Id = "grp-leg-002", DisplayName = "Compliance", Description = "Regulatory compliance and governance group" }
        },
        ["IT"] = new List<GroupDto>
        {
            new GroupDto { Id = "grp-it-001", DisplayName = "IT Team", Description = "Information Technology department team" },
            new GroupDto { Id = "grp-it-002", DisplayName = "Infrastructure", Description = "IT Infrastructure and operations group" }
        }
    };

    private static readonly List<GroupDto> _defaultGroups = new()
    {
        new GroupDto { Id = "grp-gen-001", DisplayName = "General", Description = "General all-purpose organisational group" },
        new GroupDto { Id = "grp-gen-002", DisplayName = "All Staff", Description = "All staff communication and announcements group" }
    };

    public Task<IReadOnlyList<GroupDto>> GetGroupsForDepartmentAsync(string department, CancellationToken ct = default)
    {
        var groups = _departmentGroups.TryGetValue(department, out var departmentGroupList)
            ? departmentGroupList
            : _defaultGroups;

        return Task.FromResult<IReadOnlyList<GroupDto>>(groups.AsReadOnly());
    }

    public Task AssignUserToGroupsAsync(string userId, IReadOnlyList<string> groupIds, CancellationToken ct = default)
    {
        // No-op in demo mode — groups are conceptually assigned without side effects
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GroupDto>> GetUserGroupsAsync(string userId, CancellationToken ct = default)
    {
        // Return 1-2 groups for any user
        var groups = new List<GroupDto>
        {
            new GroupDto { Id = "grp-gen-002", DisplayName = "All Staff", Description = "All staff communication and announcements group" },
            new GroupDto { Id = "grp-hr-001", DisplayName = "HR Team", Description = "Human Resources department team for workforce management" }
        };

        return Task.FromResult<IReadOnlyList<GroupDto>>(groups.AsReadOnly());
    }
}
