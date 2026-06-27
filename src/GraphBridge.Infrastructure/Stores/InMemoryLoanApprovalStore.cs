using System.Collections.Concurrent;
using GraphBridge.Application.LoanApprovals;
using GraphBridge.Domain.Entities;

namespace GraphBridge.Infrastructure.Stores;

/// <summary>
/// In-memory implementation of <see cref="ILoanApprovalStore"/> for Demo_Mode.
/// Uses ConcurrentDictionary for thread-safe storage and is pre-seeded with realistic demo data.
/// </summary>
public class InMemoryLoanApprovalStore : ILoanApprovalStore
{
    private readonly ConcurrentDictionary<Guid, LoanApproval> _loans = new();

    public InMemoryLoanApprovalStore()
    {
        // Pre-seed with Greenway Property Holdings loan for demo purposes
        var greenwayLoan = new LoanApproval
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CustomerName = "Greenway Property Holdings",
            Amount = 750000.00m,
            PropertyReference = "GP-2024-001",
            Status = "Approved",
            PackGenerated = false,
            AuditEntries = new List<LoanAuditEntry>()
        };

        _loans[greenwayLoan.Id] = greenwayLoan;
    }

    public Task<LoanApproval?> GetByIdAsync(Guid id)
    {
        _loans.TryGetValue(id, out var loan);
        return Task.FromResult(loan);
    }

    public Task<IReadOnlyList<LoanApproval>> GetAllAsync()
    {
        IReadOnlyList<LoanApproval> loans = _loans.Values.ToList().AsReadOnly();
        return Task.FromResult(loans);
    }

    public Task AddAsync(LoanApproval loan)
    {
        _loans[loan.Id] = loan;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LoanApproval loan)
    {
        _loans[loan.Id] = loan;
        return Task.CompletedTask;
    }
}
