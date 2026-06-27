---
inclusion: auto
---

# GraphBridge Enterprise Suite — Testing Standards

## Testing Stack
- **Unit Tests:** xUnit
- **Mocking:** Moq
- **Assertions:** FluentAssertions
- **Integration Tests:** WebApplicationFactory
- **Frontend Tests:** Jasmine / Karma or Jest

## What Must Be Tested

### Backend
- CQRS command handlers (business logic)
- CQRS query handlers (data retrieval)
- Validators (validation rules)
- Graph service abstractions (both real and mock)
- API response wrapper
- Demo/mock services return realistic data
- Key business workflow orchestration

### Frontend
- Components (render, inputs, outputs)
- Services (API call structure)
- Store (if NgRx — reducers, selectors, effects)

## Test Naming Convention
```
MethodName_Scenario_ExpectedResult

// Examples:
CreateEmployee_WithValidData_ReturnsCreatedDto
SendWelcomeEmail_InDemoMode_ReturnsPreviewDto
GetCeoDashboard_WithMockData_ReturnsAllSections
```

## Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task CreateEmployee_WithValidData_ReturnsCreatedDto()
{
    // Arrange
    var command = new CreateEmployeeOnboardingCommand { Name = "Sarah Khan" };
    _mockGraphUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>(), default))
        .ReturnsAsync(new UserDto { DisplayName = "Sarah Khan" });

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.EmployeeName.Should().Be("Sarah Khan");
}
```

## Integration Test Examples
- API endpoints return correct response envelope
- Demo mode returns realistic data
- Error responses are structured correctly
- Swagger documentation is accurate

## Coverage Expectations
- Business logic handlers: 90%+
- Validators: 100%
- Mock Graph services: tested for realistic data
- API endpoints: integration test coverage
