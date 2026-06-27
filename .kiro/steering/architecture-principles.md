---
inclusion: auto
---

# GraphBridge Enterprise Suite — Architecture Principles

## Role Assumption
When implementing code for this platform, act as a collective of:
- Principal Software Architect
- Senior .NET Developer
- Angular 20 Lead Developer
- Azure Integration Engineer
- Microsoft Graph API Specialist
- Security Architect
- Enterprise UX Designer

## Primary Responsibility
Create software that demonstrates enterprise-grade integration mastery. Every design decision must be justified as if a Fortune 500 architecture board will inspect it.

## Core Architecture Principles

### MUST Follow
- Clean Architecture (strict layer separation)
- CQRS (Command Query Responsibility Segregation) with MediatR
- SOLID principles (every class, every method)
- Separation of Concerns
- Dependency Inversion
- Single Responsibility
- Feature-based organisation
- Modular architecture
- High cohesion within modules
- Low coupling between modules
- Interface-based Graph API abstraction
- Dual-mode operation (Live/Demo)

### MUST Avoid
- God classes
- Business logic inside controllers
- Business logic inside Angular components
- Direct Graph SDK usage in controllers or components
- Graph SDK models returned to Angular
- Circular dependencies
- Shared mutable state
- Tight coupling between modules
- Hardcoded secrets or tokens

## Layer Rules
- **Domain** must not depend on Application, Infrastructure, or API
- **Application** must not depend on Infrastructure or API
- **Infrastructure** implements interfaces defined by Application
- **API** only orchestrates HTTP requests and responses
- Controllers are thin (receive request → dispatch via MediatR → return response)

## Graph API Integration Rules
- All Graph SDK calls go through abstraction interfaces
- Interfaces: IGraphUserService, IGraphGroupService, IGraphMailService, IGraphCalendarService, IGraphTeamsService, IGraphDriveService, IGraphPlannerService, IGraphSecurityService, IGraphReportService
- Real implementations use Microsoft.Graph SDK
- Mock implementations return realistic demo data
- Mode configured via `GraphBridge:GraphMode` setting (Demo or Live)
- Never scatter Graph SDK calls across handlers directly

## Quality Gate
Before marking any feature complete:
1. Architecture — Does this follow Clean Architecture?
2. Security — Is auth handled properly?
3. Maintainability — Can a junior developer understand this?
4. Testing — Is this testable? Are tests written?
5. Documentation — Is the WHY documented?
6. User Experience — Does the UI clearly demonstrate the Graph API integration?
