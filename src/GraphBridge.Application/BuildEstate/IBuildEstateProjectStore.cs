using GraphBridge.Domain.Entities;

namespace GraphBridge.Application.BuildEstate;

/// <summary>
/// Abstraction for BuildEstate project entity persistence.
/// </summary>
public interface IBuildEstateProjectStore
{
    Task<BuildEstateProject?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<BuildEstateProject>> GetAllAsync();
    Task AddAsync(BuildEstateProject project);
    Task UpdateAsync(BuildEstateProject project);
}
