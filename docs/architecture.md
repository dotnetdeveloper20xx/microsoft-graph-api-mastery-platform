# Architecture

## Overview

GraphBridge Enterprise Suite follows **Clean Architecture** with strict compile-time layer separation. The backend is built with ASP.NET Core (.NET 8) and uses CQRS (Command Query Responsibility Segregation) via MediatR for all business operations. This architecture ensures testability, maintainability, and clear separation of concerns across all six business modules.

---

## Clean Architecture Layers

```
┌─────────────────────────────────────────────────────┐
│                   GraphBridge.Api                     │
│          ASP.NET Core Controllers + Middleware        │
├─────────────────────────────────────────────────────┤
│               GraphBridge.Infrastructure             │
│     Graph SDK Implementations + Mock Services        │
├─────────────────────────────────────────────────────┤
│               GraphBridge.Application                │
│       CQRS Handlers + DTOs + Interfaces              │
├─────────────────────────────────────────────────────┤
│                 GraphBridge.Domain                    │
│           Entities + Enums + Value Objects            │
├─────────────────────────────────────────────────────┤
│                 GraphBridge.Shared                    │
│     API Envelope + Custom Exceptions + Constants     │
└─────────────────────────────────────────────────────┘
```

### GraphBridge.Domain

The innermost layer containing business entities, enums, and value objects. This layer has **zero external project references** — it depends on nothing else in the solution.

**Responsibilities:**
- Entity definitions (EmployeeOnboarding, LegalMatter, LoanApproval, BuildEstateProject)
- Enumerations (OnboardingStep, LoanStatus, PlanningStatus)
- Value objects (LoanAuditEntry)

### GraphBridge.Application

The application logic layer containing CQRS handlers, DTOs, validators, and service interface definitions. References Domain only.

**Responsibilities:**
- Command and Query definitions (MediatR IRequest implementations)
- Command and Query handlers (MediatR IRequestHandler implementations)
- FluentValidation validators for all inbound requests
- Graph service interface definitions (IGraphUserService, IGraphMailService, etc.)
- Data Transfer Objects (DTOs) for API responses
- MediatR pipeline behaviors (ValidationBehavior, LoggingBehavior)

### GraphBridge.Infrastructure

The implementation layer containing concrete Graph SDK integrations and mock services. References Application and Domain.

**Responsibilities:**
- Live Graph service implementations (using Microsoft Graph SDK)
- Mock Graph service implementations (deterministic demo data)
- In-memory data stores for Demo_Mode entity persistence
- MSAL token acquisition and caching
- DI registration extension methods

### GraphBridge.Api

The outermost layer containing HTTP controllers, middleware, and application startup. References Application, Infrastructure, and Shared.

**Responsibilities:**
- ASP.NET Core controllers (thin — delegate to MediatR)
- CorrelationId middleware (unique GUID per request)
- Global exception handling middleware
- Program.cs configuration and DI setup
- HTTP pipeline configuration

### GraphBridge.Shared

A cross-cutting utilities layer with no project references. Used by Api for response formatting.

**Responsibilities:**
- ApiEnvelope<T> response wrapper with static factory methods
- Custom exception types (NotFoundException, BusinessRuleException, GraphServiceException, AuthenticationException)
- Constants and shared utilities

---

## Dependency Rules (Compile-Time Enforced)

| Layer | References |
|-------|-----------|
| Domain | None (pure) |
| Application | Domain only |
| Infrastructure | Application + Domain |
| Api | Application + Infrastructure + Shared |
| Shared | None (utility) |

These rules are enforced via .csproj `<ProjectReference>` entries. The Domain layer never references any other project, ensuring business entities remain free of framework dependencies.

---

## CQRS Pattern with MediatR

Every API operation flows through MediatR's pipeline:

```
HTTP Request → Controller → mediator.Send(Command/Query) → Pipeline Behaviors → Handler → Response
```

### Commands (Write Operations)

Commands represent state-changing operations. Each command has:
- A **Command class** implementing `IRequest<TResponse>` with the input data
- A **CommandHandler** implementing `IRequestHandler<TCommand, TResponse>` with the business logic
- A **CommandValidator** implementing `AbstractValidator<TCommand>` with validation rules

Example: `CreateEmployeeCommand` → `CreateEmployeeCommandHandler` → returns `EmployeeOnboardingDto`

### Queries (Read Operations)

Queries represent data retrieval operations. Each query has:
- A **Query class** implementing `IRequest<TResponse>` with filter/lookup criteria
- A **QueryHandler** implementing `IRequestHandler<TQuery, TResponse>` with data access logic

Example: `GetCeoOverviewQuery` → `GetCeoOverviewQueryHandler` → returns `CeoDashboardOverviewDto`

### Pipeline Behaviors

MediatR pipeline behaviors execute in order for every request:

| Order | Behavior | Purpose |
|-------|----------|---------|
| 1 | `ValidationBehavior<TReq, TRes>` | Runs FluentValidation rules; throws ValidationException on failure |
| 2 | `LoggingBehavior<TReq, TRes>` | Logs request entry, exit, and elapsed time with correlationId |

The validation behavior ensures invalid requests are rejected **before** the handler executes, preventing any side effects from malformed data.

---

## Dependency Injection Strategy

### Dual-Mode Registration

At application startup, the DI container reads `GraphBridge:GraphMode` from configuration:

- **"Demo"** → Registers mock Graph service implementations + in-memory stores
- **"Live"** → Registers live Graph SDK implementations + MSAL token services
- **Missing/Invalid** → Defaults to Demo_Mode with a warning log

```csharp
// Simplified registration flow
if (graphMode == "Live")
{
    services.AddScoped<IGraphUserService, LiveGraphUserService>();
    services.AddScoped<IGraphMailService, LiveGraphMailService>();
    // ... all 9 services
}
else
{
    services.AddScoped<IGraphUserService, MockGraphUserService>();
    services.AddScoped<IGraphMailService, MockGraphMailService>();
    // ... all 9 services
    services.AddSingleton<InMemoryEmployeeStore>();
    // ... all in-memory stores
}
```

### Service Registrations

| Category | Lifetime | Registrations |
|----------|----------|---------------|
| Graph Services (9) | Scoped | One per Graph API area |
| In-Memory Stores | Singleton | One per module (Demo_Mode) |
| MediatR | Scoped | Handlers auto-discovered |
| FluentValidation | Scoped | Validators auto-discovered |
| Pipeline Behaviors | Scoped | Validation + Logging |
| Middleware | Singleton | CorrelationId + GlobalException |

---

## Module Organisation

Each module follows a consistent folder structure within the Application layer:

```
Application/
├── {ModuleName}/
│   ├── Commands/
│   │   ├── {ActionName}/
│   │   │   ├── {ActionName}Command.cs
│   │   │   ├── {ActionName}CommandHandler.cs
│   │   │   └── {ActionName}CommandValidator.cs
│   ├── Queries/
│   │   ├── {QueryName}/
│   │   │   ├── {QueryName}Query.cs
│   │   │   └── {QueryName}QueryHandler.cs
│   └── Dtos/
│       └── {ModuleName}Dto.cs
├── Interfaces/
│   └── Graph/
│       ├── IGraphUserService.cs
│       ├── IGraphGroupService.cs
│       ├── IGraphMailService.cs
│       ├── IGraphCalendarService.cs
│       ├── IGraphTeamsService.cs
│       ├── IGraphDriveService.cs
│       ├── IGraphPlannerService.cs
│       ├── IGraphSecurityService.cs
│       └── IGraphReportService.cs
```

### Module CQRS Handler Summary

| Module | Commands | Queries |
|--------|----------|---------|
| Employee Onboarding | CreateEmployee, AssignGroups, SendWelcomeEmail, ScheduleInduction | GetOverview, GetEmployeeById, GetEmployeeStatus |
| Legal Matter Workspace | CreateMatter, CreateWorkspace, InviteParticipants, ScheduleKickoff | GetOverview, GetMatterById, GetDocuments |
| Loan Approval Hub | CreateLoanApproval, GeneratePack, SendCustomerEmail, NotifyTeam, ScheduleFollowUp | GetOverview, GetLoanById, GetAudit |
| BuildEstate Project | CreateProject, LaunchWorkspace, CreateTaskBoard, NotifyDirectors, ScheduleKickoff | GetOverview, GetProjectById, GetWeeklyReport |
| CEO Command Centre | (none — read-only) | GetOverview, GetToday, GetEmails, GetCalendar, GetTasks, GetDocuments, GetSecuritySignals |
| Productivity Assistant | GenerateWeeklySummary | GetOverview, GetContextPackage, GetCalendar, GetEmails, GetTasks, GetDocuments |

---

## Request Flow

### Successful Command Flow

```
1. Angular Frontend sends HTTP POST
2. CorrelationIdMiddleware assigns unique GUID, adds to logging scope
3. Controller receives request, binds model
4. Controller calls mediator.Send(command) — single line, no business logic
5. ValidationBehavior runs FluentValidation rules — passes
6. LoggingBehavior logs request entry
7. CommandHandler executes business logic via injected Graph services
8. Graph service (mock or live) returns data
9. Handler constructs response DTO
10. Controller wraps in ApiEnvelope.Ok() and returns HTTP 200/201
```

### Failed Validation Flow

```
1. Angular Frontend sends HTTP POST with invalid data
2. CorrelationIdMiddleware assigns unique GUID
3. Controller receives request, binds model
4. Controller calls mediator.Send(command)
5. ValidationBehavior runs FluentValidation rules — FAILS
6. ValidationBehavior throws ValidationException
7. GlobalExceptionMiddleware catches exception
8. Returns HTTP 400 with ApiEnvelope.Fail() containing field errors
9. Handler is NEVER invoked
```

### Unhandled Exception Flow

```
1. Request processing encounters unexpected error
2. GlobalExceptionMiddleware catches exception
3. Logs full stack trace + correlationId to Serilog
4. Returns HTTP 500 with ApiEnvelope.Fail() — no internal details exposed
5. Client can reference correlationId for support
```
