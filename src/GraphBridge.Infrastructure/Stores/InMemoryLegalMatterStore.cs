using System.Collections.Concurrent;
using GraphBridge.Application.LegalMatters;
using GraphBridge.Domain.Entities;

namespace GraphBridge.Infrastructure.Stores;

/// <summary>
/// In-memory implementation of <see cref="ILegalMatterStore"/> for Demo_Mode.
/// Uses ConcurrentDictionary for thread-safe storage and is pre-seeded with realistic demo data.
/// </summary>
public class InMemoryLegalMatterStore : ILegalMatterStore
{
    private readonly ConcurrentDictionary<Guid, LegalMatter> _matters = new();

    public InMemoryLegalMatterStore()
    {
        // Pre-seed with Oakfield Estates matter for demo purposes
        var oakfieldMatter = new LegalMatter
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ReferenceNumber = "LM-A7F3BC21",
            ClientName = "Oakfield Estates Ltd",
            MatterType = "Commercial Property",
            AssignedSolicitor = "Emma Thompson",
            WorkspaceCreated = true,
            Participants = new List<string> { "emma.thompson@graphbridge.com", "david.chen@graphbridge.com" }
        };

        _matters[oakfieldMatter.Id] = oakfieldMatter;
    }

    public Task<LegalMatter?> GetByIdAsync(Guid id)
    {
        _matters.TryGetValue(id, out var matter);
        return Task.FromResult(matter);
    }

    public Task<IReadOnlyList<LegalMatter>> GetAllAsync()
    {
        IReadOnlyList<LegalMatter> matters = _matters.Values.ToList().AsReadOnly();
        return Task.FromResult(matters);
    }

    public Task AddAsync(LegalMatter matter)
    {
        _matters[matter.Id] = matter;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LegalMatter matter)
    {
        _matters[matter.Id] = matter;
        return Task.CompletedTask;
    }
}
