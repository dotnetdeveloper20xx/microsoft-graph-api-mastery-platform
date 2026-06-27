# Requirements Document

## Introduction

GraphBridge Enterprise Suite is a portfolio-grade Microsoft Graph API Mastery Platform comprising six distinct business application modules. Each module demonstrates enterprise-level integration between custom business workflows and Microsoft 365 services via the Microsoft Graph API. The platform runs in dual mode (Live with real Graph SDK credentials, or Demo with realistic mock data) and follows Clean Architecture with CQRS patterns throughout.

## Glossary

- **Platform**: The GraphBridge Enterprise Suite application as a whole, encompassing backend API, frontend Angular application, and all six modules
- **Module**: One of the six self-contained business application units within the Platform, each with its own backend endpoints, CQRS handlers, and Angular feature pages
- **Graph_Service_Layer**: The set of abstraction interfaces (IGraphUserService, IGraphMailService, etc.) that encapsulate all Microsoft Graph API interactions
- **Demo_Mode**: The operational mode where the Platform returns realistic mock data without requiring Microsoft Graph credentials
- **Live_Mode**: The operational mode where the Platform uses the Microsoft Graph SDK with authenticated access to Microsoft 365
- **API_Envelope**: The standardised JSON response wrapper containing success, message, data, errors, timestamp, and correlationId fields
- **CQRS_Handler**: A MediatR-based command or query handler that processes a single business operation
- **Master_Dashboard**: The Angular landing page displaying all six module cards with navigation
- **Onboarding_Module**: Module 1 — Employee Onboarding Automation
- **Legal_Matter_Module**: Module 2 — Legal Matter Workspace Automation
- **Loan_Approval_Module**: Module 3 — Loan Approval Communication Hub
- **BuildEstate_Module**: Module 4 — BuildEstate Project Launch Workspace
- **CEO_Dashboard_Module**: Module 5 — CEO Command Centre Dashboard
- **Productivity_Module**: Module 6 — AI Meeting & Productivity Assistant
- **Angular_Frontend**: The Angular 20 single-page application with standalone components, signals, and lazy-loaded feature routes
- **Clean_Architecture**: The layered architecture pattern with Domain, Application, Infrastructure, and API layers with strict dependency rules

## Requirements

### Requirement 1: Solution Architecture Foundation

**User Story:** As a developer, I want the Platform to follow Clean Architecture with strict layer separation, so that the codebase is maintainable, testable, and demonstrates enterprise-grade design.

#### Acceptance Criteria

1. THE Platform SHALL organise backend code into five projects: GraphBridge.Api, GraphBridge.Application, GraphBridge.Domain, GraphBridge.Infrastructure, and GraphBridge.Shared
2. THE Platform SHALL enforce that the Domain layer has zero project references to GraphBridge.Application, GraphBridge.Infrastructure, or GraphBridge.Api, verified by the .csproj file containing no such `<ProjectReference>` entries
3. THE Platform SHALL enforce that the Application layer has zero project references to GraphBridge.Infrastructure or GraphBridge.Api, verified by the .csproj file containing no such `<ProjectReference>` entries
4. THE Platform SHALL use MediatR for dispatching all commands and queries from API controllers to CQRS_Handlers, with a MediatR pipeline behavior registered for FluentValidation that automatically validates inbound requests before handler execution
5. THE Platform SHALL use FluentValidation for validating all inbound request objects, with at least one AbstractValidator implementation per command request type that accepts external input
6. WHEN a controller receives an HTTP request, THE Platform SHALL delegate processing to a CQRS_Handler via a single `mediator.Send()` call, with the controller method body containing only model binding, the MediatR send call, and HTTP response mapping (no conditional business logic, data transformation, or direct service calls)
7. IF a FluentValidation validator rejects a request, THEN THE Platform SHALL return the validation failure in the API_Envelope format without invoking the CQRS_Handler

### Requirement 2: Consistent API Response Envelope

**User Story:** As a frontend developer, I want all API responses to follow a consistent structure, so that I can build predictable error handling and data binding in the Angular application.

#### Acceptance Criteria

1. THE Platform SHALL wrap every API endpoint response in the API_Envelope format containing: success (boolean), message (string, maximum 500 characters), data (object or null), errors (array of objects each with field and detail string properties), timestamp (ISO 8601 UTC string), and correlationId (GUID string)
2. WHEN a request succeeds, THE Platform SHALL return HTTP 200 (or 201 for resource creation) with success as true, the result object in the data field, an empty errors array, and a message describing the completed action
3. WHEN a request fails due to FluentValidation errors, THE Platform SHALL return HTTP 400 with success as false, message set to "Validation failed", and the errors array containing one entry per validation failure with the field name and detail description
4. WHEN an unhandled exception occurs, THE Platform SHALL return HTTP 500 with success as false, message set to "An unexpected error occurred. Please reference the correlationId for support.", data as null, and SHALL log the full exception stack trace, exception message, and correlationId to the structured logging sink
5. THE Platform SHALL generate a unique GUID correlationId for each inbound HTTP request and include it in all structured log entries (via a logging scope or enricher) for the duration of that request
6. IF a requested resource is not found, THEN THE Platform SHALL return HTTP 404 with success as false, message indicating the resource type and identifier that was not found, and an empty errors array

### Requirement 3: Dual-Mode Graph API Operation

**User Story:** As a developer evaluating this platform, I want to run the full application without Microsoft Graph credentials, so that I can see realistic demo workflows immediately.

#### Acceptance Criteria

1. THE Platform SHALL support two operational modes configurable via the `GraphBridge:GraphMode` setting: "Live" and "Demo"
2. WHILE the Platform operates in Demo_Mode, THE Graph_Service_Layer SHALL return hardcoded data representing named business entities (persons, organisations, dates, and document names) for all Graph API operations without making any external HTTP or network calls
3. WHILE the Platform operates in Live_Mode, THE Graph_Service_Layer SHALL authenticate with Microsoft Entra ID using MSAL and execute Microsoft Graph SDK calls against the configured tenant
4. THE Platform SHALL provide a mock implementation for each Graph service interface: IGraphUserService, IGraphGroupService, IGraphMailService, IGraphCalendarService, IGraphTeamsService, IGraphDriveService, IGraphPlannerService, IGraphSecurityService, and IGraphReportService
5. WHEN the Platform starts in Demo_Mode, THE Platform SHALL not require any Microsoft Entra ID configuration values (TenantId, ClientId, ClientSecret) to be present, and SHALL start successfully with those values absent or empty
6. THE Platform SHALL register either real or mock service implementations via dependency injection at startup based on the GraphMode configuration value, with the mode fixed for the lifetime of the application instance
7. IF the `GraphBridge:GraphMode` setting is absent or set to a value other than "Live" or "Demo", THEN THE Platform SHALL default to Demo_Mode and log a warning indicating the unrecognised configuration value

### Requirement 4: Graph API Abstraction Layer

**User Story:** As a developer, I want all Graph API calls isolated behind interfaces, so that the application is testable and the Graph SDK never leaks into controllers or Angular components.

#### Acceptance Criteria

1. THE Platform SHALL define abstraction interfaces in the Application layer for each Graph API area: IGraphUserService, IGraphGroupService, IGraphMailService, IGraphCalendarService, IGraphTeamsService, IGraphDriveService, IGraphPlannerService, IGraphSecurityService, and IGraphReportService
2. THE Platform SHALL implement each interface in the Infrastructure layer using the Microsoft Graph SDK for Live_Mode, with each implementation class residing in the GraphBridge.Infrastructure project
3. THE Platform SHALL implement each interface in the Infrastructure layer using hardcoded realistic data for Demo_Mode, with each mock implementation class residing in the GraphBridge.Infrastructure project
4. THE Platform SHALL ensure no Microsoft Graph SDK types (from the Microsoft.Graph namespace) appear in API controller method signatures, controller return types, or response DTO classes, verifiable by the GraphBridge.Api project having no package reference to Microsoft.Graph
5. WHEN a CQRS_Handler requires Graph API data, THE CQRS_Handler SHALL obtain it exclusively through the injected Graph service interface, with no direct instantiation of Graph SDK clients within handlers
6. IF a Graph service call fails in Live_Mode (network error, authentication failure, or Graph API error response), THEN THE Graph_Service_Layer SHALL throw a typed exception containing the operation name and failure reason, allowing the global exception handler to return an API_Envelope error response

### Requirement 5: Employee Onboarding Automation Module

**User Story:** As an HR administrator, I want to create a new employee and trigger automated Microsoft 365 onboarding, so that the new hire has groups, a welcome email, and an induction meeting scheduled without manual intervention.

#### Acceptance Criteria

1. WHEN an HR administrator submits a new employee onboarding request with name (1-100 characters), role (1-100 characters), department (1-50 characters), manager name (1-100 characters), and email address (valid email format), THE Onboarding_Module SHALL create an employee profile and return the record with a system-generated unique GUID identifier
2. IF a create employee request is missing any required field or contains invalid data, THEN THE Onboarding_Module SHALL reject the request with a 400 validation error listing each invalid field
3. WHEN the assign-groups action is triggered for an employee, THE Onboarding_Module SHALL assign the employee to at least one Microsoft 365 group determined by the employee's department value, using the IGraphGroupService
4. WHEN the send-welcome-email action is triggered for an employee, THE Onboarding_Module SHALL compose and send a welcome email to the employee's stored email address using the IGraphMailService, with the email containing the employee's name and role
5. WHEN the schedule-induction action is triggered for an employee, THE Onboarding_Module SHALL create a calendar event of 60 minutes duration using the IGraphCalendarService, with the employee and their manager as attendees
6. WHEN the onboarding status is requested for an employee, THE Onboarding_Module SHALL return the completion state of each onboarding step as a boolean for each of: profileCreated, groupsAssigned, welcomeEmailSent, inductionScheduled
7. IF an onboarding action endpoint is called with an employee ID that does not exist, THEN THE Onboarding_Module SHALL return a 404 response indicating the employee was not found
8. THE Onboarding_Module SHALL expose endpoints at: GET /api/onboarding/overview, POST /api/onboarding/employees, GET /api/onboarding/employees/{id}, POST /api/onboarding/employees/{id}/assign-groups, POST /api/onboarding/employees/{id}/send-welcome-email, POST /api/onboarding/employees/{id}/schedule-induction, GET /api/onboarding/employees/{id}/status

### Requirement 6: Legal Matter Workspace Automation Module

**User Story:** As a solicitor, I want to open a new legal matter and have the system create a Microsoft 365 workspace, so that I have a SharePoint folder structure, Teams channel, participant access, and kickoff meeting configured automatically.

#### Acceptance Criteria

1. WHEN a solicitor creates a new legal matter, THE Legal_Matter_Module SHALL store the matter with client name (maximum 200 characters), matter type, assigned solicitor, and a system-generated unique reference number, and return the created record with its identifier
2. WHEN the create-workspace action is triggered, THE Legal_Matter_Module SHALL create a SharePoint folder structure for the matter containing at minimum top-level folders for Correspondence, Contracts, Evidence, and Notes using the IGraphDriveService
3. WHEN the create-workspace action is triggered, THE Legal_Matter_Module SHALL create a Teams channel concept for the matter named after the matter reference number using the IGraphTeamsService
4. IF the create-workspace action is triggered for a matter that already has a workspace, THEN THE Legal_Matter_Module SHALL return an error response indicating the workspace already exists without creating duplicate resources
5. WHEN the invite-participants action is triggered with a list of at least one and at most 50 participant email addresses, THE Legal_Matter_Module SHALL invite specified internal and external participants to the workspace and return the count of successfully invited participants
6. IF the invite-participants action is triggered with an empty participants list or more than 50 participants, THEN THE Legal_Matter_Module SHALL return a validation error indicating the allowed range of 1 to 50 participants
7. WHEN the schedule-kickoff action is triggered, THE Legal_Matter_Module SHALL create a calendar event for the matter kickoff meeting within 14 calendar days of the request using the IGraphCalendarService, including all invited participants as attendees
8. WHEN documents are requested for a matter, THE Legal_Matter_Module SHALL return the document folder tree structure for the workspace including folder names and nested hierarchy
9. THE Legal_Matter_Module SHALL expose endpoints at: GET /api/legal-matters/overview, POST /api/legal-matters, GET /api/legal-matters/{id}, POST /api/legal-matters/{id}/create-workspace, POST /api/legal-matters/{id}/invite-participants, POST /api/legal-matters/{id}/schedule-kickoff, GET /api/legal-matters/{id}/documents

### Requirement 7: Loan Approval Communication Hub Module

**User Story:** As a loan processor, I want the system to automate communication workflows when a loan is approved, so that the customer, underwriter, and finance team are all notified with consistent, professional communications.

#### Acceptance Criteria

1. WHEN a loan approval is recorded, THE Loan_Approval_Module SHALL store the loan details including customer name (maximum 200 characters), amount (between 0.01 and 999,999,999.99 in GBP), property reference (maximum 100 characters), and approval status
2. WHEN the generate-pack action is triggered, THE Loan_Approval_Module SHALL create a communication pack containing customer email content with subject and body, internal notification content with summary text, and a document checklist of required follow-up items
3. IF the generate-pack action is triggered for a loan that does not have "Approved" status, THEN THE Loan_Approval_Module SHALL return an error response indicating that communication packs can only be generated for approved loans
4. WHEN the send-customer-email action is triggered, THE Loan_Approval_Module SHALL send the approval notification email to the customer using the IGraphMailService and record the send timestamp in the audit trail
5. IF the send-customer-email action is triggered before a communication pack has been generated, THEN THE Loan_Approval_Module SHALL return an error response indicating the communication pack must be generated first
6. WHEN the notify-team action is triggered, THE Loan_Approval_Module SHALL send a Teams-style notification to the internal finance channel using the IGraphTeamsService and record the notification timestamp in the audit trail
7. WHEN the schedule-follow-up action is triggered, THE Loan_Approval_Module SHALL create a calendar event for the customer follow-up meeting using the IGraphCalendarService and record the scheduling timestamp in the audit trail
8. WHEN the audit trail is requested, THE Loan_Approval_Module SHALL return a chronological list of all communication actions taken for the loan approval, each entry containing action type, timestamp, and status, limited to the most recent 100 entries
9. THE Loan_Approval_Module SHALL expose endpoints at: GET /api/loan-approvals/overview, POST /api/loan-approvals, GET /api/loan-approvals/{id}, POST /api/loan-approvals/{id}/generate-pack, POST /api/loan-approvals/{id}/send-customer-email, POST /api/loan-approvals/{id}/notify-team, POST /api/loan-approvals/{id}/schedule-follow-up, GET /api/loan-approvals/{id}/audit

### Requirement 8: BuildEstate Project Launch Workspace Module

**User Story:** As a project manager, I want to launch a full Microsoft 365 project workspace when planning approval is granted, so that the development team has a task board, SharePoint documents, director notifications, and kickoff scheduling in place immediately.

#### Acceptance Criteria

1. WHEN a planning approval is recorded, THE BuildEstate_Module SHALL store the project with name (maximum 200 characters), location (maximum 200 characters), planning status, and at least one assigned director
2. WHEN the launch-workspace action is triggered, THE BuildEstate_Module SHALL create a SharePoint folder structure for the project containing at minimum top-level folders for Planning Documents, Contracts, Site Reports, and Financial using the IGraphDriveService
3. IF the launch-workspace action is triggered for a project that already has a workspace, THEN THE BuildEstate_Module SHALL return an error response indicating the workspace already exists without creating duplicate resources
4. WHEN the create-task-board action is triggered, THE BuildEstate_Module SHALL create a Planner-style task board with at least 3 default buckets (To Do, In Progress, Completed) and at least 3 initial tasks distributed across buckets using the IGraphPlannerService
5. WHEN the notify-directors action is triggered, THE BuildEstate_Module SHALL send notification emails to all assigned directors (between 1 and 20) using the IGraphMailService and return the count of notifications sent
6. IF the notify-directors action is triggered for a project with no assigned directors, THEN THE BuildEstate_Module SHALL return an error response indicating at least one director must be assigned before notifications can be sent
7. WHEN the schedule-kickoff action is triggered, THE BuildEstate_Module SHALL create a project kickoff calendar event for all team members using the IGraphCalendarService, scheduled within 14 calendar days of the request
8. WHEN a weekly report is requested, THE BuildEstate_Module SHALL return a summary containing: count of tasks by status (to do, in progress, completed), list of milestones due within the next 7 days, and count of team activity events in the past 7 days
9. THE BuildEstate_Module SHALL expose endpoints at: GET /api/buildestate-projects/overview, POST /api/buildestate-projects, GET /api/buildestate-projects/{id}, POST /api/buildestate-projects/{id}/launch-workspace, POST /api/buildestate-projects/{id}/create-task-board, POST /api/buildestate-projects/{id}/notify-directors, POST /api/buildestate-projects/{id}/schedule-kickoff, GET /api/buildestate-projects/{id}/weekly-report

### Requirement 9: CEO Command Centre Dashboard Module

**User Story:** As a CEO, I want a single dashboard that aggregates signals from across Microsoft 365, so that I can see meetings, emails, tasks, documents, and security alerts in one executive view.

#### Acceptance Criteria

1. WHEN the CEO accesses the command centre overview endpoint, THE CEO_Dashboard_Module SHALL return an overview summary containing: count of today's meetings (calendar day in user's timezone), count of unread emails, count of pending tasks, count of pending document approvals, and count of active security signals from the past 24 hours
2. WHEN the today endpoint is called, THE CEO_Dashboard_Module SHALL return today's calendar events (from midnight to end of day in user's timezone) using the IGraphCalendarService, limited to a maximum of 50 events
3. WHEN the emails endpoint is called, THE CEO_Dashboard_Module SHALL return a summary of emails received within the past 24 hours grouped by priority (high, normal, low) using the IGraphMailService, limited to a maximum of 50 email summaries
4. WHEN the tasks endpoint is called, THE CEO_Dashboard_Module SHALL return pending tasks and their status (not started, in progress, overdue) using the IGraphPlannerService, limited to a maximum of 50 tasks
5. WHEN the documents endpoint is called, THE CEO_Dashboard_Module SHALL return documents pending approval or modified within the past 7 days using the IGraphDriveService, limited to a maximum of 50 documents
6. WHEN the security-signals endpoint is called, THE CEO_Dashboard_Module SHALL return security alerts and sign-in anomalies from the past 24 hours using the IGraphSecurityService, limited to a maximum of 50 signals
7. IF any individual Graph service call fails during overview aggregation, THEN THE CEO_Dashboard_Module SHALL return partial results with the available data and include an error indicator for the unavailable section rather than failing the entire request
8. THE CEO_Dashboard_Module SHALL expose endpoints at: GET /api/ceo-command-centre/overview, GET /api/ceo-command-centre/today, GET /api/ceo-command-centre/emails, GET /api/ceo-command-centre/calendar, GET /api/ceo-command-centre/tasks, GET /api/ceo-command-centre/documents, GET /api/ceo-command-centre/security-signals

### Requirement 10: AI Meeting and Productivity Assistant Module

**User Story:** As a knowledge worker, I want to receive a weekly productivity summary gathered from Microsoft 365 data, so that I can review my calendar, email, tasks, and document activity in one AI-ready context package.

#### Acceptance Criteria

1. WHEN a weekly summary is requested, THE Productivity_Module SHALL aggregate calendar events, email summaries, task completions, and document activity for the past 7 calendar days (from current date minus 7 days to current date)
2. WHEN the context-package endpoint is called, THE Productivity_Module SHALL return a structured JSON object containing sections for calendar, emails, tasks, and documents, each with their respective aggregated data from the past 7 days, suitable for AI consumption
3. WHEN the calendar endpoint is called, THE Productivity_Module SHALL return calendar events for the current week (Monday to Sunday) using the IGraphCalendarService, limited to a maximum of 100 events
4. WHEN the emails endpoint is called, THE Productivity_Module SHALL return email volume (total sent and received counts), top 10 senders by message count, and unread count for the past 7 days using the IGraphMailService
5. WHEN the tasks endpoint is called, THE Productivity_Module SHALL return counts of tasks completed, tasks overdue, and tasks in progress for the past 7 days using the IGraphPlannerService
6. WHEN the documents endpoint is called, THE Productivity_Module SHALL return documents accessed or modified within the past 7 days using the IGraphDriveService, limited to a maximum of 50 documents
7. IF any individual Graph service call fails during weekly summary aggregation, THEN THE Productivity_Module SHALL return partial results with available data and include an error indicator for the unavailable section rather than failing the entire request
8. THE Productivity_Module SHALL expose endpoints at: GET /api/productivity-assistant/overview, POST /api/productivity-assistant/weekly-summary, GET /api/productivity-assistant/context-package, GET /api/productivity-assistant/calendar, GET /api/productivity-assistant/emails, GET /api/productivity-assistant/tasks, GET /api/productivity-assistant/documents

### Requirement 11: Angular Frontend Architecture

**User Story:** As a user, I want a professional enterprise dashboard UI with module navigation, loading states, and clear explanations of Graph API usage, so that I can explore each module's capabilities effectively.

#### Acceptance Criteria

1. THE Angular_Frontend SHALL use Angular 20 with standalone components, signals for local state, and lazy-loaded feature routes for each of the six modules (Onboarding, Legal Matters, Loan Approvals, BuildEstate Projects, CEO Command Centre, Productivity Assistant)
2. THE Angular_Frontend SHALL display a Master_Dashboard at the /dashboard route showing six module cards, each containing the module name, a business purpose description of at least one sentence, a list of Graph API areas used by that module, and a navigation button that routes to the module landing page
3. WHEN a user navigates to a module, THE Angular_Frontend SHALL display loading skeleton placeholder components within 200 milliseconds of navigation start, and SHALL replace the skeletons with actual content or an error state within 10 seconds
4. WHEN an API call returns an HTTP error status (4xx or 5xx), THE Angular_Frontend SHALL display an error state component containing a message describing the failure category (network error, validation error, or server error) and a retry action button that re-invokes the failed API call
5. WHEN the backend returns an API_Envelope with an empty data field or an empty array for a module view, THE Angular_Frontend SHALL display an empty state component containing text explaining what data would appear and a primary action button that navigates to the relevant creation or configuration action
6. THE Angular_Frontend SHALL include a Graph API explanation panel on each module page containing: the names of the Graph API endpoints demonstrated, the permission scopes required, and a one-sentence description of each Graph capability used
7. THE Angular_Frontend SHALL use HTTP interceptors that perform three functions: attach the correlationId header to every outgoing request, emit a loading state signal when a request is in-flight, and route HTTP error responses to a centralised error handling service
8. WHEN a user accesses the root URL (/), THE Angular_Frontend SHALL redirect to the /dashboard route

### Requirement 12: Security and Secret Management

**User Story:** As a security-conscious developer, I want the Platform to handle authentication tokens and secrets securely, so that no credentials are exposed in source code and the application is ready for production deployment.

#### Acceptance Criteria

1. THE Platform SHALL store no secrets, tokens, or certificates in source code or configuration files committed to version control, and SHALL include appsettings.Development.json in the .gitignore file to prevent accidental commits
2. THE Platform SHALL use appsettings.Development.json placeholder values for Microsoft Entra ID configuration (TenantId, ClientId, ClientSecret, RedirectUri) where placeholder values are clearly non-functional strings such as "YOUR-TENANT-ID-HERE"
3. THE Platform SHALL include documentation at /docs/authentication-and-permissions.md containing a dedicated section explaining Azure Key Vault integration for production secret management, including steps for configuring managed identity access
4. WHEN operating in Live_Mode, THE Platform SHALL acquire access tokens from Microsoft Entra ID using the Microsoft Identity platform (MSAL) and SHALL cache tokens until 5 minutes before their expiry time
5. IF the access token is expired, invalid, or token acquisition fails due to network or configuration error, THEN THE Platform SHALL return a 401 Unauthorized response with the API_Envelope format containing success as false, an error message indicating the authentication failure category, and the correlationId in the response and log output

### Requirement 13: Testing Infrastructure

**User Story:** As a developer maintaining this codebase, I want comprehensive unit tests for handlers, validators, and mock services, so that I can refactor with confidence and demonstrate testability.

#### Acceptance Criteria

1. THE Platform SHALL include at least one unit test per CQRS_Handler verifying that the handler invokes the expected Graph service interface methods and returns a correctly structured DTO
2. THE Platform SHALL include at least two unit tests per FluentValidation validator: one verifying acceptance of valid input and one verifying rejection of invalid input with appropriate error messages
3. THE Platform SHALL include at least one unit test per mock Graph service implementation verifying that the returned demo data contains all required DTO fields with non-null, non-empty values
4. THE Platform SHALL include at least one integration test per module verifying that the overview endpoint returns an HTTP 200 response with a valid API_Envelope containing success as true and a non-null data field when running in Demo_Mode
5. THE Platform SHALL organise tests into GraphBridge.UnitTests and GraphBridge.IntegrationTests projects, where all unit tests run without network access and all integration tests use the TestServer with Demo_Mode configuration

### Requirement 14: Demo Data Realism

**User Story:** As a portfolio reviewer, I want the demo data to represent realistic business scenarios, so that the platform feels genuine and professional when demonstrated without live Graph credentials.

#### Acceptance Criteria

1. WHILE operating in Demo_Mode, THE Onboarding_Module SHALL return deterministic demo data representing employee Sarah Khan, role Junior Business Analyst, department Finance, manager Afzal Ahmed, with at least three assigned groups and a completed onboarding status showing all four steps (profile created, groups assigned, welcome email sent, induction scheduled)
2. WHILE operating in Demo_Mode, THE Legal_Matter_Module SHALL return deterministic demo data representing matter "Commercial Lease Review" for client Oakfield Estates Ltd, solicitor Emma Roberts, with a workspace containing at least three SharePoint folders and at least two invited participants
3. WHILE operating in Demo_Mode, THE Loan_Approval_Module SHALL return deterministic demo data representing customer Greenway Property Holdings, loan amount £1,250,000, status Approved, with a communication pack containing customer email content, team notification content, and an audit trail of at least three chronological entries
4. WHILE operating in Demo_Mode, THE BuildEstate_Module SHALL return deterministic demo data representing project "Riverside Heights", planning status Approved, location Reading, with a task board containing at least three buckets and at least five tasks, and at least two assigned directors
5. WHILE operating in Demo_Mode, THE CEO_Dashboard_Module SHALL return deterministic demo data including at least three meetings for today, at least five unread emails, at least two security alerts, and at least three pending approval items
6. WHILE operating in Demo_Mode, THE Productivity_Module SHALL return deterministic demo data including at least five weekly calendar events, email summary with sender counts and unread totals, at least three completed tasks, and at least four recently modified documents

### Requirement 15: Documentation and Developer Onboarding

**User Story:** As a developer joining this project, I want comprehensive documentation covering architecture, Graph API concepts, authentication, and module guides, so that I can understand and extend the platform quickly.

#### Acceptance Criteria

1. THE Platform SHALL include documentation at /docs/architecture.md containing sections explaining: the Clean Architecture layer responsibilities (Domain, Application, Infrastructure, API, Shared), the CQRS pattern with MediatR dispatch flow, the dependency injection registration strategy, and the module folder organisation
2. THE Platform SHALL include documentation at /docs/microsoft-graph-overview.md containing sections explaining: what Microsoft Graph API is, which Microsoft 365 services it exposes, and how each of the six modules demonstrates specific Graph API capabilities
3. THE Platform SHALL include documentation at /docs/authentication-and-permissions.md containing sections explaining: Entra ID app registration steps, the permissions model with a table of all required scopes, the difference between delegated and application permissions, admin consent requirements, and Key Vault integration for production
4. THE Platform SHALL include documentation at /docs/module-guide.md containing an entry for each of the six modules with: business scenario description, list of backend endpoints, list of Graph API areas used, and list of Angular routes
5. THE Platform SHALL include documentation at /docs/endpoint-catalog.md listing every API endpoint with HTTP method, route path, request body example (where applicable), and a response body example showing the API_Envelope structure
6. THE Platform SHALL include a README.md containing: project name and one-paragraph description, architecture overview with layer diagram in text or ASCII format, technology stack list, prerequisites and setup instructions for running both backend and frontend, Demo_Mode explanation, a table listing all six modules with a one-line description each, and a link to the /docs folder for detailed documentation
