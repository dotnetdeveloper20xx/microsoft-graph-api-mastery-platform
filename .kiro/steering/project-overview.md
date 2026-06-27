---
inclusion: auto
---

# GraphBridge Enterprise Suite — Project Overview

## Platform Identity
- **Name:** GraphBridge Enterprise Suite
- **Alternative:** Microsoft Graph API Enterprise Integration Platform
- **Type:** Portfolio-Grade Microsoft Graph API Mastery Platform
- **Purpose:** Demonstrate advanced enterprise integration skills using Microsoft Graph API

## Technology Stack
- **Backend:** ASP.NET Core, Clean Architecture, CQRS with MediatR, FluentValidation
- **Frontend:** Angular 20, Signals, NgRx/Signal-based state management
- **Auth:** Microsoft Entra ID (Azure AD), JWT Bearer tokens
- **Integration:** Microsoft Graph API (Users, Groups, Mail, Calendar, Teams, SharePoint, OneDrive, Planner, Security)
- **API Design:** RESTful, Swagger/OpenAPI, consistent response envelope
- **Testing:** xUnit, FluentAssertions, Moq, WebApplicationFactory

## The Six Application Modules

| # | Module | Business Scenario | Graph API Areas |
|---|--------|-------------------|-----------------|
| 1 | Employee Onboarding Automation | HR automates M365 onboarding | Users, Groups, Mail, Calendar |
| 2 | Legal Matter Workspace Automation | Solicitor creates M365 workspace | SharePoint, Teams, Calendar |
| 3 | Loan Approval Communication Hub | Loan approved, notify stakeholders | Mail, Teams, OneDrive, Calendar |
| 4 | BuildEstate Project Launch Workspace | Planning approved, launch workspace | Planner, Teams, SharePoint, Calendar |
| 5 | CEO Command Centre Dashboard | Executive overview from M365 | All Graph APIs aggregated |
| 6 | AI Meeting & Productivity Assistant | Weekly productivity summary | Calendar, Mail, Tasks, Files |

## Dual-Mode Operation
- **Live Mode:** Uses real Microsoft Graph SDK with proper authentication
- **Demo Mode:** Returns realistic mock data without Graph credentials

## Key Design Decisions
- Graph API calls are behind interfaces (IGraphUserService, IGraphMailService, etc.)
- Mock implementations exist for all Graph services
- Configuration toggles between Live and Demo mode
- No secrets hardcoded; Key Vault in production
- Controllers are thin; CQRS handlers contain orchestration logic
- Graph SDK models never leak to the frontend

## Solution Structure

```
/backend
  /src
    /GraphBridge.Api
    /GraphBridge.Application
    /GraphBridge.Domain
    /GraphBridge.Infrastructure
    /GraphBridge.Shared
  /tests
    /GraphBridge.UnitTests
    /GraphBridge.IntegrationTests

/frontend
  /src/app
    /core
    /shared
    /layout
    /features
      /onboarding
      /legal-matters
      /loan-approvals
      /buildestate-projects
      /ceo-command-centre
      /productivity-assistant
    /state
    /models
    /services

/docs
```

## Definition of Done
1. Backend solution builds successfully
2. Frontend Angular app builds successfully
3. Swagger shows all module endpoints
4. Dashboard shows all six applications
5. Each module has at least one end-to-end demo workflow
6. Demo mode works without Microsoft Graph credentials
7. API responses are consistent
8. Angular pages call backend and display results
9. Loading, error, and empty states exist
10. Documentation exists in /docs
11. README.md sells the project as a portfolio platform
12. No secrets hardcoded
13. Code follows Clean Architecture
14. The project looks professional enough to show in an interview
