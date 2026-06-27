---
inclusion: auto
---

# GraphBridge Enterprise Suite — Backend Standards

## Technology
- ASP.NET Core (latest stable)
- C# (latest language features)
- MediatR (CQRS dispatcher)
- FluentValidation (command/query validation)
- Microsoft.Graph SDK (Graph API integration)
- Azure.Identity (Entra ID authentication)

## Async Requirements
- Async all the way — no `.Result`, no `.Wait()`, no `Task.Run()` for IO
- CancellationToken on every async method signature
- Pass CancellationToken down to Graph SDK calls, HTTP clients, file operations

## Controller Standards
Controllers must be **thin**. A controller must ONLY:
1. Receive the HTTP request
2. Dispatch a command or query via MediatR
3. Return an appropriate HTTP response

Controllers must NEVER:
- Contain business logic
- Call Graph SDK directly
- Call repositories directly
- Perform validation (validators handle this)
- Catch exceptions (global middleware handles this)
- Return Graph SDK models directly

## CQRS Standards
Every module must contain:
- **Commands** — Represent intent to change state
- **Queries** — Represent intent to read state
- **Handlers** — Execute logic, call Graph service interfaces
- **Validators** — Validate input before handling
- **DTOs** — Data transfer objects for API boundaries

### Feature Folder Structure
```
/Application
  /{Module}
    /Commands
      /{CommandName}
        {CommandName}Command.cs
        {CommandName}CommandHandler.cs
        {CommandName}CommandValidator.cs
    /Queries
      /{QueryName}
        {QueryName}Query.cs
        {QueryName}QueryHandler.cs
    /Dtos
```

## API Response Envelope
Every endpoint must return:
```json
{
  "success": true,
  "message": "Description of result",
  "data": {},
  "errors": [],
  "timestamp": "2026-06-26T10:00:00Z",
  "correlationId": "guid"
}
```

## API Design
- Route prefix: `/api`
- Use Swagger/OpenAPI with clear descriptions
- Group endpoints by module
- RESTful resource naming
- Proper HTTP status codes: 200, 201, 204, 400, 401, 403, 404, 500

## Graph Service Abstraction Pattern
```csharp
// Interface (in Application layer)
public interface IGraphUserService
{
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
    Task<UserDto> GetUserAsync(string userId, CancellationToken ct);
    Task AssignGroupsAsync(string userId, IEnumerable<string> groupIds, CancellationToken ct);
}

// Real implementation (in Infrastructure layer)
public class GraphUserService : IGraphUserService { ... }

// Mock implementation (in Infrastructure layer)
public class MockGraphUserService : IGraphUserService { ... }
```

## Dependency Injection
- Register by interface, resolve by interface
- Use configuration to select Real vs Mock implementations
- Scoped lifetime for request-bound services
- Singleton for configuration and caches

## Exception Handling
- Global exception handler middleware
- Structured logging with correlation IDs
- Never expose internal details in error messages
- Use Result pattern or consistent ApiResponse wrapper

## Structured Logging
```csharp
_logger.LogInformation(
    "Employee {EmployeeId} onboarding initiated by {UserId}",
    employeeId, currentUser.Id);
```
