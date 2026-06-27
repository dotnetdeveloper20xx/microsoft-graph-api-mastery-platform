using GraphBridge.Application.Dtos.Graph;
using GraphBridge.Application.Interfaces.Graph;
using GraphBridge.Shared.Exceptions;
using MediatR;

namespace GraphBridge.Application.Onboarding.Commands.ScheduleInduction;

/// <summary>
/// Handles scheduling an induction meeting for a new employee.
/// Creates a 60-minute calendar event with the employee and their manager as attendees.
/// </summary>
public class ScheduleInductionCommandHandler : IRequestHandler<ScheduleInductionCommand, Unit>
{
    private readonly IEmployeeStore _employeeStore;
    private readonly IGraphCalendarService _graphCalendarService;

    public ScheduleInductionCommandHandler(IEmployeeStore employeeStore, IGraphCalendarService graphCalendarService)
    {
        _employeeStore = employeeStore;
        _graphCalendarService = graphCalendarService;
    }

    public async Task<Unit> Handle(ScheduleInductionCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeStore.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(60);

        var calendarRequest = new CreateCalendarEventRequest
        {
            Subject = "Induction Meeting",
            Start = startTime,
            End = endTime,
            Attendees = new List<string> { employee.Email, $"{employee.ManagerName.ToLower().Replace(" ", ".")}@company.com" }
        };

        await _graphCalendarService.CreateEventAsync(calendarRequest, cancellationToken);

        employee.InductionScheduled = true;
        await _employeeStore.UpdateAsync(employee);

        return Unit.Value;
    }
}
