using System.Collections.Concurrent;
using GraphBridge.Application.Onboarding;
using GraphBridge.Domain.Entities;

namespace GraphBridge.Infrastructure.Stores;

/// <summary>
/// In-memory implementation of <see cref="IEmployeeStore"/> for Demo_Mode.
/// Uses ConcurrentDictionary for thread-safe storage and is pre-seeded with realistic demo data.
/// </summary>
public class InMemoryEmployeeStore : IEmployeeStore
{
    private readonly ConcurrentDictionary<Guid, EmployeeOnboarding> _employees = new();

    public InMemoryEmployeeStore()
    {
        // Pre-seed with Sarah Khan record for demo purposes
        var sarahKhan = new EmployeeOnboarding
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Sarah Khan",
            Role = "Senior Software Engineer",
            Department = "IT",
            ManagerName = "James Wilson",
            Email = "sarah.khan@graphbridge.com",
            ProfileCreated = true,
            GroupsAssigned = true,
            WelcomeEmailSent = true,
            InductionScheduled = false
        };

        _employees[sarahKhan.Id] = sarahKhan;
    }

    public Task<EmployeeOnboarding?> GetByIdAsync(Guid id)
    {
        _employees.TryGetValue(id, out var employee);
        return Task.FromResult(employee);
    }

    public Task<IReadOnlyList<EmployeeOnboarding>> GetAllAsync()
    {
        IReadOnlyList<EmployeeOnboarding> employees = _employees.Values.ToList().AsReadOnly();
        return Task.FromResult(employees);
    }

    public Task AddAsync(EmployeeOnboarding employee)
    {
        _employees[employee.Id] = employee;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EmployeeOnboarding employee)
    {
        _employees[employee.Id] = employee;
        return Task.CompletedTask;
    }
}
