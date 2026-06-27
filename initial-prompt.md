You are now acting as a Principal Software Architect, Technical Project Manager, Senior .NET Developer, Angular 20 Lead Developer, Azure Integration Engineer, Microsoft Graph API Specialist, and Portfolio Product Designer.

Your task is to build a complete portfolio-grade Microsoft Graph API Mastery Platform inside the existing local repository/folder.

The purpose of this project is to demonstrate advanced enterprise integration skills using:

- ASP.NET Core
- Clean Architecture
- CQRS with MediatR
- Microsoft Graph API
- Microsoft Entra ID authentication
- Azure identity concepts
- Angular 20
- Signals
- NgRx or signal-based state management where appropriate
- Enterprise dashboard UX
- Secure API design
- Modular backend/frontend structure
- Real-world business workflows

This must not be a toy demo.

It should look and feel like a serious enterprise integration platform that a senior developer could present in an interview, on a CV, or to a CTO.

====================================================
PROJECT NAME
====================================================

GraphBridge Enterprise Suite

Alternative display name:

Microsoft Graph API Enterprise Integration Platform

Purpose:

A single solution containing six different Microsoft Graph-powered business applications, each implemented as its own module, with separate backend endpoints and separate Angular pages.

The platform must show how Microsoft Graph API can connect custom business applications with Microsoft 365, Entra ID, Outlook, Teams, SharePoint, OneDrive, Planner, Calendar, Users, Groups, and security data.

====================================================
THE SIX APPLICATION MODULES
====================================================

Build the following six modules inside one solution.

Each module must have:

- Backend API endpoints
- Application layer CQRS commands/queries
- DTOs
- Validators where appropriate
- Domain/service abstractions
- Infrastructure Graph API implementation
- Angular feature module/page
- Dashboard cards
- Loading/error/empty states
- Mock/demo mode if real Graph credentials are not configured
- Clear explanation text on the UI showing what the module demonstrates
- Results rendered in Angular 20 UI

----------------------------------------------------
MODULE 1: Employee Onboarding Automation
----------------------------------------------------

Business Scenario:

HR creates a new employee. The system automates Microsoft 365 onboarding.

Developer Value:

Shows user management, groups, licenses, mailbox/calendar preparation, Teams/SharePoint setup concepts.

Capabilities to demonstrate:

- Create/read user profile
- Assign user to groups
- Prepare onboarding checklist
- Create welcome email draft
- Schedule induction meeting
- Show assigned groups
- Show onboarding status

Backend endpoints:

GET    /api/onboarding/overview
POST   /api/onboarding/employees
GET    /api/onboarding/employees/{id}
POST   /api/onboarding/employees/{id}/assign-groups
POST   /api/onboarding/employees/{id}/send-welcome-email
POST   /api/onboarding/employees/{id}/schedule-induction
GET    /api/onboarding/employees/{id}/status

Angular pages:

/onboarding
/onboarding/new
/onboarding/:id
/onboarding/:id/status

UI should show:

- Employee profile
- Group assignment checklist
- Welcome email preview
- Calendar induction meeting preview
- Automation timeline
- Graph API calls used

----------------------------------------------------
MODULE 2: Legal Matter Workspace Automation
----------------------------------------------------

Business Scenario:

A solicitor opens a new legal matter. The system creates a Microsoft 365 workspace around the case.

Developer Value:

Shows document workspace automation, Teams collaboration, SharePoint folder structure, secure access, calendar events.

Capabilities to demonstrate:

- Create matter workspace
- Create SharePoint folder structure
- Create Teams channel concept
- Invite internal/external participants
- Schedule matter kickoff meeting
- Show document/security checklist

Backend endpoints:

GET    /api/legal-matters/overview
POST   /api/legal-matters
GET    /api/legal-matters/{id}
POST   /api/legal-matters/{id}/create-workspace
POST   /api/legal-matters/{id}/invite-participants
POST   /api/legal-matters/{id}/schedule-kickoff
GET    /api/legal-matters/{id}/documents

Angular pages:

/legal-matters
/legal-matters/new
/legal-matters/:id
/legal-matters/:id/workspace

UI should show:

- Matter summary
- Client/solicitor information
- Workspace creation status
- SharePoint folder tree
- Teams channel preview
- Participants list
- Security notes

----------------------------------------------------
MODULE 3: Loan Approval Communication Hub
----------------------------------------------------

Business Scenario:

A loan is approved. The system uses Graph API to notify the customer, underwriter, finance team, and create supporting workspace assets.

Developer Value:

Shows Outlook email automation, Teams notifications, document storage, calendar follow-up, finance workflow integration.

Capabilities to demonstrate:

- Generate approval communication pack
- Send or preview email
- Notify internal Teams channel concept
- Create approval folder
- Schedule customer follow-up
- Show workflow audit trail

Backend endpoints:

GET    /api/loan-approvals/overview
POST   /api/loan-approvals
GET    /api/loan-approvals/{id}
POST   /api/loan-approvals/{id}/generate-pack
POST   /api/loan-approvals/{id}/send-customer-email
POST   /api/loan-approvals/{id}/notify-team
POST   /api/loan-approvals/{id}/schedule-follow-up
GET    /api/loan-approvals/{id}/audit

Angular pages:

/loan-approvals
/loan-approvals/new
/loan-approvals/:id
/loan-approvals/:id/audit

UI should show:

- Loan summary
- Customer email preview
- Internal Teams notification preview
- Follow-up meeting details
- Approval document checklist
- Audit timeline

----------------------------------------------------
MODULE 4: BuildEstate Project Launch Workspace
----------------------------------------------------

Business Scenario:

A planning application is approved in a property development project. The system launches a Microsoft 365 project workspace.

Developer Value:

Shows enterprise project collaboration, planner/task automation, Teams/SharePoint integration, director notifications, calendar setup.

Capabilities to demonstrate:

- Create project workspace
- Create Planner-style task board concept
- Create SharePoint folder structure
- Notify directors
- Schedule project kickoff
- Assign project roles
- Generate weekly report placeholder

Backend endpoints:

GET    /api/buildestate-projects/overview
POST   /api/buildestate-projects
GET    /api/buildestate-projects/{id}
POST   /api/buildestate-projects/{id}/launch-workspace
POST   /api/buildestate-projects/{id}/create-task-board
POST   /api/buildestate-projects/{id}/notify-directors
POST   /api/buildestate-projects/{id}/schedule-kickoff
GET    /api/buildestate-projects/{id}/weekly-report

Angular pages:

/buildestate-projects
/buildestate-projects/new
/buildestate-projects/:id
/buildestate-projects/:id/workspace
/buildestate-projects/:id/report

UI should show:

- Project summary
- Planning approval status
- Workspace setup checklist
- Planner task board preview
- Team assignments
- Director notification preview
- Weekly report preview

----------------------------------------------------
MODULE 5: CEO Command Centre Dashboard
----------------------------------------------------

Business Scenario:

A CEO wants one dashboard that pulls together useful signals from Microsoft 365.

Developer Value:

Shows how Graph API can aggregate Outlook, Calendar, Teams, Planner, SharePoint, Users, and security-style signals into one executive dashboard.

Capabilities to demonstrate:

- Show today’s meetings
- Show unread email summary
- Show pending document approvals concept
- Show project/task status
- Show recent Teams/channel activity concept
- Show user activity/security alerts concept
- Show executive summary cards

Backend endpoints:

GET /api/ceo-command-centre/overview
GET /api/ceo-command-centre/today
GET /api/ceo-command-centre/emails
GET /api/ceo-command-centre/calendar
GET /api/ceo-command-centre/tasks
GET /api/ceo-command-centre/documents
GET /api/ceo-command-centre/security-signals

Angular pages:

/ceo-command-centre
/ceo-command-centre/today
/ceo-command-centre/activity
/ceo-command-centre/security

UI should show:

- Executive dashboard
- Today’s meetings
- Email highlights
- Task/project status
- Document approvals
- Security/activity signals
- “What Graph API provided this data?” panel

----------------------------------------------------
MODULE 6: AI Meeting & Productivity Assistant
----------------------------------------------------

Business Scenario:

A user asks for a weekly productivity summary. The system gathers Microsoft 365 context from calendar, emails, Teams-style messages, tasks, and documents.

Developer Value:

Shows Microsoft Graph as the data foundation for AI/copilot-style applications.

Capabilities to demonstrate:

- Fetch weekly calendar events
- Fetch email summary concept
- Fetch meeting notes/transcripts placeholder
- Fetch task summary
- Fetch document activity
- Produce AI-ready context package
- Show generated summary placeholder
- Optional future Azure OpenAI integration abstraction

Backend endpoints:

GET  /api/productivity-assistant/overview
POST /api/productivity-assistant/weekly-summary
GET  /api/productivity-assistant/context-package
GET  /api/productivity-assistant/calendar
GET  /api/productivity-assistant/emails
GET  /api/productivity-assistant/tasks
GET  /api/productivity-assistant/documents

Angular pages:

/productivity-assistant
/productivity-assistant/weekly-summary
/productivity-assistant/context-package

UI should show:

- Weekly summary dashboard
- Calendar summary
- Email summary
- Task summary
- Document activity
- AI context package JSON preview
- Generated executive summary placeholder

====================================================
SOLUTION STRUCTURE
====================================================

Create a clean, professional folder structure.

Recommended backend structure:

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

Recommended frontend structure:

/frontend
  /src
    /app
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

Documentation structure:

/docs
  architecture.md
  microsoft-graph-overview.md
  authentication-and-permissions.md
  module-guide.md
  endpoint-catalog.md
  frontend-guide.md
  developer-onboarding.md
  demo-mode.md
  definition-of-done.md

====================================================
BACKEND ARCHITECTURE
====================================================

Use Clean Architecture.

Layers:

1. Domain
2. Application
3. Infrastructure
4. API
5. Shared

Rules:

- Domain must not depend on Application, Infrastructure, or API.
- Application must not depend on Infrastructure or API.
- Infrastructure implements interfaces defined by Application.
- API only orchestrates HTTP requests and responses.
- Keep controllers thin.
- Use CQRS with MediatR.
- Use DTOs for all external API contracts.
- Use FluentValidation for input validation.
- Use Result pattern or consistent ApiResponse wrapper.
- Use global exception handling middleware.
- Use structured logging.
- Use dependency injection properly.
- Avoid business logic in controllers.
- Avoid returning EF entities directly.
- Avoid leaking Graph SDK models directly to the frontend.

====================================================
MICROSOFT GRAPH INTEGRATION DESIGN
====================================================

Create an abstraction over Microsoft Graph.

Do not scatter Graph SDK calls across handlers.

Create interfaces such as:

IGraphUserService
IGraphGroupService
IGraphMailService
IGraphCalendarService
IGraphTeamsService
IGraphDriveService
IGraphPlannerService
IGraphSecurityService
IGraphReportService

Infrastructure implementations:

GraphUserService
GraphGroupService
GraphMailService
GraphCalendarService
GraphTeamsService
GraphDriveService
GraphPlannerService
GraphSecurityService
GraphReportService

Also create mock implementations:

MockGraphUserService
MockGraphGroupService
MockGraphMailService
MockGraphCalendarService
MockGraphTeamsService
MockGraphDriveService
MockGraphPlannerService
MockGraphSecurityService
MockGraphReportService

The app must support:

- Real Graph mode
- Demo/mock mode

Configuration:

GraphBridge:
  GraphMode: Demo

or

GraphBridge:
  GraphMode: Live

When GraphMode is Demo, return realistic demo data.

When GraphMode is Live, use Microsoft Graph SDK with proper authentication.

====================================================
AUTHENTICATION AND AUTHORIZATION
====================================================

Use Microsoft Entra ID concepts.

Backend should be prepared for JWT bearer authentication.

Create documentation and configuration placeholders for:

- Tenant ID
- Client ID
- Client Secret or certificate
- Redirect URI
- Scopes
- App permissions
- Delegated permissions
- Admin consent

Support these Graph permission categories in documentation:

- User.Read
- User.Read.All
- Group.Read.All
- Group.ReadWrite.All
- Mail.Read
- Mail.Send
- Calendars.Read
- Calendars.ReadWrite
- Files.Read.All
- Files.ReadWrite.All
- Sites.Read.All
- Sites.ReadWrite.All
- Team.ReadBasic.All
- Channel.ReadBasic.All
- Tasks.ReadWrite
- Directory.Read.All
- AuditLog.Read.All

Important:

Do not hardcode secrets.
Do not store tokens in source code.
Use appsettings.Development.json placeholders only.
Explain how Key Vault would be used in production.

====================================================
CQRS REQUIREMENTS
====================================================

Each module must use Commands and Queries.

Example structure:

/Application
  /Onboarding
    /Commands
      /CreateEmployeeOnboarding
      /AssignEmployeeGroups
      /SendWelcomeEmail
      /ScheduleInductionMeeting
    /Queries
      /GetOnboardingOverview
      /GetEmployeeOnboardingById
      /GetEmployeeOnboardingStatus
    /Dtos
    /Validators

Apply the same pattern to all six modules.

Each command/query should have:

- Request object
- Handler
- DTO response
- Validator if input is involved
- Unit test where practical

====================================================
API DESIGN REQUIREMENTS
====================================================

Use RESTful endpoints.

Use route prefix:

/api

Use Swagger/OpenAPI.

Group endpoints by module.

Add clear Swagger descriptions.

Every endpoint response should include:

- success
- message
- data
- errors
- timestamp
- correlationId

Example:

{
  "success": true,
  "message": "Employee onboarding profile created.",
  "data": {},
  "errors": [],
  "timestamp": "2026-06-26T10:00:00Z",
  "correlationId": "..."
}

====================================================
FRONTEND REQUIREMENTS
====================================================

Use Angular 20.

Use standalone components where appropriate.

Use Angular Signals for local component state.

Use NgRx or signal store for feature-level state where it adds value.

Use RxJS properly for API calls.

Use typed API clients/services.

Use route-based lazy loading.

Use guards where appropriate.

Use interceptors for:

- Auth token attachment placeholder
- Error handling
- Loading indicator
- Correlation ID

Create a polished enterprise layout:

- Left navigation
- Top bar
- Dashboard landing page
- Module cards
- Breadcrumbs
- Status badges
- Timeline components
- Empty state components
- Error state components
- Loading skeletons
- Graph API call explanation panels

Recommended frontend structure per feature:

/features/onboarding
  onboarding.routes.ts
  pages/
  components/
  services/
  models/
  store/

Do this for all modules.

====================================================
UI/UX REQUIREMENTS
====================================================

The UI must look like a serious Microsoft/Azure enterprise dashboard.

Style direction:

- Clean
- Modern
- Calm
- Professional
- Azure-inspired
- Light theme
- Strong spacing
- Clear typography
- Dashboard cards
- Icons
- Status chips
- Timelines
- Tables
- Detail panels

Each module landing page must include:

- What this module does
- Why Microsoft Graph matters here
- Business workflow diagram or visual stepper
- Key Graph API areas used
- Demo actions
- Results panel

Each action button should show:

- What action was triggered
- Which backend endpoint was called
- Which Graph API capability this represents
- Demo result

====================================================
MASTER DASHBOARD
====================================================

Create a home dashboard at:

/dashboard

It should show six large module cards:

1. Employee Onboarding Automation
2. Legal Matter Workspace Automation
3. Loan Approval Communication Hub
4. BuildEstate Project Launch Workspace
5. CEO Command Centre Dashboard
6. AI Meeting & Productivity Assistant

Each card should show:

- Business purpose
- Graph APIs used
- Number of demo workflows
- Status
- Open button

Also include a global “How Microsoft Graph Works” panel:

User/Application
→ Microsoft Entra ID
→ Access Token
→ Microsoft Graph API
→ Microsoft 365 Data/Actions
→ Business Workflow Result

====================================================
DATA MODELS
====================================================

Create clean DTOs.

Examples:

EmployeeOnboardingDto
CreateEmployeeOnboardingRequest
EmployeeGroupAssignmentDto
WelcomeEmailPreviewDto
InductionMeetingDto
OnboardingStatusDto

LegalMatterDto
CreateLegalMatterRequest
MatterWorkspaceDto
MatterParticipantDto
MatterDocumentTreeDto

LoanApprovalDto
CreateLoanApprovalRequest
ApprovalCommunicationPackDto
CustomerEmailPreviewDto
LoanWorkflowAuditDto

BuildEstateProjectDto
CreateBuildEstateProjectRequest
ProjectWorkspaceDto
ProjectTaskBoardDto
DirectorNotificationDto
WeeklyProjectReportDto

CeoDashboardDto
CalendarSummaryDto
EmailSummaryDto
TaskSummaryDto
DocumentApprovalDto
SecuritySignalDto

ProductivitySummaryRequest
ProductivitySummaryDto
AiContextPackageDto
CalendarContextDto
EmailContextDto
TaskContextDto
DocumentContextDto

====================================================
DEMO DATA REQUIREMENTS
====================================================

If live Graph credentials are not configured, app must still run fully.

Create realistic demo data.

Use business-realistic examples.

For example:

Employee:
Name: Sarah Khan
Role: Junior Business Analyst
Department: Finance
Manager: Afzal Ahmed

Legal Matter:
Matter: Commercial Lease Review
Client: Oakfield Estates Ltd
Solicitor: Emma Roberts

Loan:
Customer: Greenway Property Holdings
Amount: £1,250,000
Status: Approved

BuildEstate:
Project: Riverside Heights
Planning Status: Approved
Location: Reading

CEO Dashboard:
Show meetings, unread emails, security alerts, pending approvals.

Productivity Assistant:
Show weekly calendar, email summaries, tasks, and AI-ready context.

====================================================
TESTING REQUIREMENTS
====================================================

Add unit tests for:

- CQRS handlers
- Validators
- Graph service abstractions
- Demo/mock services
- API response wrapper
- Key business workflow orchestration

Add integration test examples for:

- API endpoints
- Demo mode responses
- Error responses

====================================================
DOCUMENTATION REQUIREMENTS
====================================================

Create strong developer documentation.

docs/architecture.md:

Explain Clean Architecture, CQRS, module layout, frontend structure.

docs/microsoft-graph-overview.md:

Explain what Microsoft Graph API is, why developers use it, and how this project demonstrates it.

docs/authentication-and-permissions.md:

Explain delegated permissions, application permissions, admin consent, scopes, tokens, app registration, managed identity, Key Vault.

docs/module-guide.md:

Explain each of the six modules, business scenario, endpoints, Graph API areas used, frontend pages.

docs/endpoint-catalog.md:

List every backend endpoint with request/response examples.

docs/frontend-guide.md:

Explain Angular structure, state management, signals, services, routing, components.

docs/demo-mode.md:

Explain how the app runs without real Graph credentials.

docs/definition-of-done.md:

List completion criteria.

README.md:

Create a professional portfolio README with:

- Project vision
- Architecture diagram in text
- Feature list
- Modules table
- Tech stack
- How to run backend
- How to run frontend
- Demo mode explanation
- Microsoft Graph learning outcomes
- Screenshots placeholder section
- Future enhancements

====================================================
DEFINITION OF DONE
====================================================

The task is complete only when:

1. Backend solution builds successfully.
2. Frontend Angular app builds successfully.
3. Swagger opens and shows all module endpoints.
4. Dashboard page shows all six applications.
5. Each module has at least one working end-to-end demo workflow.
6. Demo mode works without Microsoft Graph credentials.
7. API responses are consistent.
8. Angular pages call the backend and display results.
9. Loading, error, and empty states exist.
10. Documentation exists in /docs.
11. README.md clearly sells the project as a Microsoft Graph API portfolio platform.
12. No secrets are hardcoded.
13. Code follows Clean Architecture.
14. Controllers are thin.
15. CQRS handlers contain orchestration logic.
16. Graph API calls are behind interfaces.
17. Mock Graph services are available.
18. Frontend uses Angular 20 patterns properly.
19. The project looks professional enough to show in an interview.
20. The codebase is understandable by another developer.

====================================================
IMPORTANT IMPLEMENTATION APPROACH
====================================================

Before writing code:

1. Inspect the entire local repository.
2. Identify whether this is an empty project or existing project.
3. Do not randomly overwrite important existing files.
4. Create or update the structure carefully.
5. Generate a short implementation plan.
6. Then implement in logical phases.

Recommended phases:

Phase 1:
Create solution structure, backend projects, frontend structure, shared docs.

Phase 2:
Create domain/application contracts, response wrappers, CQRS patterns, mock Graph services.

Phase 3:
Implement Module 1 and Module 2 end-to-end.

Phase 4:
Implement Module 3 and Module 4 end-to-end.

Phase 5:
Implement Module 5 and Module 6 end-to-end.

Phase 6:
Polish Angular dashboard, navigation, UI components, state management, loading/error states.

Phase 7:
Add tests, docs, README, final cleanup.

====================================================
CODING STANDARDS
====================================================

Follow these standards:

- Clear naming
- Small focused classes
- No god services
- No business logic in controllers
- No direct Graph SDK usage in controllers
- No direct Graph SDK models returned to Angular
- Use async/await properly
- Use cancellation tokens
- Use nullable reference types
- Use dependency injection
- Use FluentValidation
- Use typed DTOs
- Use clean folder organization
- Use consistent API response objects
- Use environment-based configuration
- Use secure secret handling
- Use least privilege permission documentation
- Use professional comments only where useful

====================================================
FINAL OUTPUT EXPECTED
====================================================

When finished, provide:

1. Summary of what was created.
2. Folder structure.
3. Backend run instructions.
4. Frontend run instructions.
5. Swagger URL.
6. Angular app URL.
7. List of implemented modules.
8. List of key endpoints.
9. Any assumptions made.
10. Any remaining TODOs.

Now begin by inspecting the local project structure and creating an implementation plan before coding.