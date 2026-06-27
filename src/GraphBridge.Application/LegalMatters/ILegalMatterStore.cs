using GraphBridge.Domain.Entities;

namespace GraphBridge.Application.LegalMatters;

/// <summary>
/// Abstraction for legal matter entity persistence.
/// </summary>
public interface ILegalMatterStore
{
    Task<LegalMatter?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<LegalMatter>> GetAllAsync();
    Task AddAsync(LegalMatter matter);
    Task UpdateAsync(LegalMatter matter);
}
