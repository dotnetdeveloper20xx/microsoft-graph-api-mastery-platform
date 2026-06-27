using GraphBridge.Application.Dtos.LoanApprovals;
using MediatR;

namespace GraphBridge.Application.LoanApprovals.Commands.GeneratePack;

/// <summary>
/// Command to generate a communication pack for an approved loan.
/// </summary>
public class GeneratePackCommand : IRequest<CommunicationPackDto>
{
    public Guid LoanId { get; set; }
}
