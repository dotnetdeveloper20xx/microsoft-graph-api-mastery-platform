using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.SendWelcomeEmail;

/// <summary>
/// Handles sending a welcome email to a new employee via IGraphMailService.
/// The email body contains the employee's name and role.
/// </summary>
public class SendWelcomeEmailCommandHandler : IRequestHandler<SendWelcomeEmailCommand, Unit>
{
    private readonly IEmployeeStore _employeeStore;
    private readonly IGraphMailService _graphMailService;

    public SendWelcomeEmailCommandHandler(IEmployeeStore employeeStore, IGraphMailService graphMailService)
    {
        _employeeStore = employeeStore;
        _graphMailService = graphMailService;
    }

    public async Task<Unit> Handle(SendWelcomeEmailCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeStore.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        var emailRequest = new SendEmailRequest
        {
            To = employee.Email,
            Subject = "Welcome to the team!",
            Body = $"Dear {employee.Name},\n\nWelcome to the team! We are thrilled to have you join us as a {employee.Role}.\n\nWe look forward to working with you.\n\nBest regards,\nHR Team"
        };

        await _graphMailService.SendEmailAsync(emailRequest, cancellationToken);

        employee.WelcomeEmailSent = true;
        await _employeeStore.UpdateAsync(employee);

        return Unit.Value;
    }
}
