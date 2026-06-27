using GraphBridge.Domain.Entities;

namespace GraphBridge.Application.Onboarding;

/// <summary>
/// Abstraction for employee onboarding data persistence.
/// </summary>
public interface IEmployeeStore
{
    Task<EmployeeOnboarding?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<EmployeeOnboarding>> GetAllAsync();
    Task AddAsync(EmployeeOnboarding employee);
    Task UpdateAsync(EmployeeOnboarding employee);
}
