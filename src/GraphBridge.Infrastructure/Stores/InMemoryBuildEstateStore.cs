using System.Collections.Concurrent;
using GraphBridge.Application.BuildEstate;
using GraphBridge.Domain.Entities;

namespace GraphBridge.Infrastructure.Stores;

/// <summary>
/// In-memory implementation of <see cref="IBuildEstateProjectStore"/> for Demo_Mode.
/// Uses ConcurrentDictionary for thread-safe storage and is pre-seeded with realistic demo data.
/// </summary>
public class InMemoryBuildEstateStore : IBuildEstateProjectStore
{
    private readonly ConcurrentDictionary<Guid, BuildEstateProject> _projects = new();

    public InMemoryBuildEstateStore()
    {
        // Pre-seed with Riverside Heights project for demo purposes
        var riversideProject = new BuildEstateProject
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Riverside Heights",
            Location = "Manchester, M1 4BT",
            PlanningStatus = "Approved",
            Directors = new List<string> { "afzal.ahmed@graphbridge.com", "james.wilson@graphbridge.com" },
            WorkspaceLaunched = false,
            TaskBoardCreated = false
        };

        _projects[riversideProject.Id] = riversideProject;
    }

    public Task<BuildEstateProject?> GetByIdAsync(Guid id)
    {
        _projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<IReadOnlyList<BuildEstateProject>> GetAllAsync()
    {
        IReadOnlyList<BuildEstateProject> projects = _projects.Values.ToList().AsReadOnly();
        return Task.FromResult(projects);
    }

    public Task AddAsync(BuildEstateProject project)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(BuildEstateProject project)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }
}
