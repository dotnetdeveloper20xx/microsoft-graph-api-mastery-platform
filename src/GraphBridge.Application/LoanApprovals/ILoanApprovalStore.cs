using GraphBridge.Domain.Entities;

namespace GraphBridge.Application.LoanApprovals;

/// <summary>
/// Abstraction for loan approval entity persistence.
/// </summary>
public interface ILoanApprovalStore
{
    Task<LoanApproval?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<LoanApproval>> GetAllAsync();
    Task AddAsync(LoanApproval loan);
    Task UpdateAsync(LoanApproval loan);
}
