# Implementation Plan: GraphBridge Enterprise Suite

## Overview

This plan implements the GraphBridge Enterprise Suite — a portfolio-grade Microsoft Graph API Mastery Platform with six business modules. The backend uses ASP.NET Core (.NET 8) with Clean Architecture and CQRS (MediatR), and the frontend uses Angular 20 with standalone components, signals, and lazy-loaded routes. Implementation proceeds layer-by-layer: solution structure → shared components → domain → application interfaces → infrastructure (mock + live) → API controllers → Angular frontend → testing → documentation.

## Tasks

- [x] 1. Set up solution structure and shared foundation
  - [x] 1.1 Create .NET solution with five backend projects and two test projects
    - Create `GraphBridge.sln` with projects: GraphBridge.Api, GraphBridge.Application, GraphBridge.Domain, GraphBridge.Infrastructure, GraphBridge.Shared, GraphBridge.UnitTests, GraphBridge.IntegrationTests
    - Configure project references enforcing dependency rules: Domain→none, Application→Domain, Infrastructure→Application+Domain, Api→Application+Infrastructure+Shared, Shared→none
    - Add NuGet packages: MediatR, FluentValidation, FluentValidation.DependencyInjectionExtensions, Microsoft.Graph, Azure.Identity, Serilog, xUnit, Moq, FluentAssertions, FsCheck.Xunit
    - _Requirements: 1.1, 1.2, 1.3, 13.5_

  - [x] 1.2 Implement API_Envelope and shared exception types in GraphBridge.Shared
    - Create `ApiEnvelope<T>` with static factory methods `Ok()` and `Fail()`
    - Create `ApiError` class with `Field` and `Detail` properties
    - Create custom exception types: `NotFoundException`, `BusinessRuleException`, `GraphServiceException`, `AuthenticationException`
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.6_

  - [x] 1.3 Implement MediatR pipeline behaviors
    - Create `ValidationBehavior<TRequest, TResponse>` that runs FluentValidation and throws `ValidationException` on failure
    - Create `LoggingBehavior<TRequest, TResponse>` that logs request entry, exit, elapsed time with correlationId
    - Register behaviors in DI with correct ordering (Validation first, then Logging)
    - _Requirements: 1.4, 1.7_

  - [x] 1.4 Implement CorrelationId middleware and GlobalExceptionMiddleware
    - Create `CorrelationIdMiddleware` that generates a unique GUID per request and adds it to logging scope
    - Create `GlobalExceptionMiddleware` that maps exceptions to correct HTTP status codes and API_Envelope responses (ValidationException→400, NotFoundException→404, BusinessRuleException→422, GraphServiceException→502, AuthenticationException→401, all others→500)
    - Ensure stack traces and internal details are never exposed in responses
    - _Requirements: 2.4, 2.5, 2.6, 4.6, 12.5_

  - [x] 1.5 Configure Program.cs with dual-mode DI registration and middleware pipeline
    - Read `GraphBridge:GraphMode` configuration value; default to "Demo" if absent/invalid with warning log
    - Register MediatR, FluentValidation validators, and pipeline behaviors
    - Conditionally register Live or Mock Graph service implementations based on mode
    - Configure middleware pipeline: CorrelationId → GlobalException → Routing → Controllers
    - Configure Serilog structured logging
    - _Requirements: 3.1, 3.5, 3.6, 3.7_

- [x] 2. Implement Domain layer entities
  - [x] 2.1 Create domain entities for all six modules
    - Create `EmployeeOnboarding` entity with Id, Name, Role, Department, ManagerName, Email, and onboarding status flags
    - Create `LegalMatter` entity with Id, ReferenceNumber, ClientName, MatterType, AssignedSolicitor, WorkspaceCreated, participants collection
    - Create `LoanApproval` entity with Id, CustomerName, Amount, PropertyReference, Status, PackGenerated, audit entries collection
    - Create `BuildEstateProject` entity with Id, Name, Location, PlanningStatus, directors collection, WorkspaceLaunched, TaskBoardCreated
    - Create `LoanAuditEntry` value object with ActionType, Timestamp, Status
    - Create enums: OnboardingStep, LoanStatus, PlanningStatus
    - _Requirements: 5.1, 6.1, 7.1, 8.1_

- [x] 3. Implement Application layer — Graph service interfaces and DTOs
  - [x] 3.1 Define all nine Graph service interfaces in Application layer
    - Create `IGraphUserService`, `IGraphGroupService`, `IGraphMailService`, `IGraphCalendarService`, `IGraphTeamsService`, `IGraphDriveService`, `IGraphPlannerService`, `IGraphSecurityService`, `IGraphReportService`
    - Place all interfaces in `Application/Interfaces/Graph/` folder
    - Define method signatures as specified in the design document
    - _Requirements: 4.1, 4.4, 4.5_

  - [x] 3.2 Create all DTOs and request/response models
    - Create Onboarding DTOs: `EmployeeOnboardingDto`, `CreateEmployeeRequest`, `OnboardingStatusDto`
    - Create Legal Matter DTOs: `LegalMatterDto`, `CreateLegalMatterRequest`, `MatterDocumentTreeDto`
    - Create Loan Approval DTOs: `LoanApprovalDto`, `CreateLoanApprovalRequest`, `CommunicationPackDto`, `EmailContentDto`, `AuditEntryDto`
    - Create BuildEstate DTOs: `BuildEstateProjectDto`, `CreateBuildEstateProjectRequest`, `TaskBoardDto`, `TaskBucketDto`, `ProjectTaskDto`, `WeeklyReportDto`
    - Create CEO Dashboard DTOs: `CeoDashboardOverviewDto`, `SectionErrorDto`, `CalendarEventDto`, `SecuritySignalDto`
    - Create Productivity DTOs: `ProductivitySummaryDto`, `EmailVolumeDto`, `SenderSummaryDto`, `TaskCompletionSummaryDto`, `AiContextPackageDto`, `DocumentDto`
    - Create shared Graph DTOs: `UserProfileDto`, `GroupDto`, `EmailSummaryDto`, `TeamChannelDto`, `FolderStructureDto`, `PlannerTaskDto`, `ActivityReportDto`, `SendEmailRequest`, `CreateCalendarEventRequest`, `CreateChannelRequest`, `SendChannelNotificationRequest`, `CreateFolderStructureRequest`, `CreateTaskBoardRequest`
    - _Requirements: 2.1, 5.1, 6.1, 7.1, 8.1, 9.1, 10.1_

  - [x] 3.3 Implement FluentValidation validators for all command requests
    - Create `CreateEmployeeRequestValidator` (Name 1-100, Role 1-100, Department 1-50, ManagerName 1-100, Email valid format)
    - Create `CreateLegalMatterRequestValidator` (ClientName max 200, MatterType required, AssignedSolicitor required)
    - Create `CreateLoanApprovalRequestValidator` (CustomerName max 200, Amount 0.01-999999999.99, PropertyReference max 100, Status required)
    - Create `CreateBuildEstateProjectRequestValidator` (Name max 200, Location max 200, PlanningStatus required, Directors at least 1)
    - Create `InviteParticipantsRequestValidator` (participants list 1-50 entries)
    - _Requirements: 1.5, 5.2, 6.6, 7.1, 8.1_

- [x] 4. Implement Application layer — CQRS handlers for Onboarding and Legal Matters modules
  - [x] 4.1 Implement Onboarding module CQRS handlers
    - Create `CreateEmployeeCommand` + `CreateEmployeeCommandHandler` — stores employee, sets profileCreated=true, returns DTO with GUID
    - Create `AssignGroupsCommand` + `AssignGroupsCommandHandler` — calls IGraphGroupService.GetGroupsForDepartment + AssignUserToGroups, updates groupsAssigned flag
    - Create `SendWelcomeEmailCommand` + `SendWelcomeEmailCommandHandler` — calls IGraphMailService.SendEmail with employee name+role in body, updates welcomeEmailSent flag
    - Create `ScheduleInductionCommand` + `ScheduleInductionCommandHandler` — calls IGraphCalendarService.CreateEvent with 60-min duration, employee+manager attendees, updates inductionScheduled flag
    - Create `GetOnboardingOverviewQuery` + handler, `GetEmployeeByIdQuery` + handler, `GetEmployeeStatusQuery` + handler
    - _Requirements: 5.1, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8_

  - [x] 4.2 Implement Legal Matters module CQRS handlers
    - Create `CreateMatterCommand` + handler — stores matter with system-generated reference number, returns DTO
    - Create `CreateWorkspaceCommand` + handler — checks workspace not already created, calls IGraphDriveService for folder structure (Correspondence, Contracts, Evidence, Notes) and IGraphTeamsService for channel named after reference number
    - Create `InviteParticipantsCommand` + handler — invites 1-50 participants, returns count
    - Create `ScheduleKickoffCommand` + handler — creates calendar event within 14 days with all participants
    - Create `GetLegalMatterOverviewQuery` + handler, `GetMatterByIdQuery` + handler, `GetDocumentsQuery` + handler
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.7, 6.8, 6.9_

- [x] 5. Implement Application layer — CQRS handlers for Loan Approvals and BuildEstate modules
  - [x] 5.1 Implement Loan Approvals module CQRS handlers
    - Create `CreateLoanApprovalCommand` + handler — stores loan details, returns DTO
    - Create `GeneratePackCommand` + handler — checks status is "Approved", generates communication pack with customer email (subject+body), internal notification, document checklist
    - Create `SendCustomerEmailCommand` + handler — checks pack is generated, calls IGraphMailService, creates audit entry with timestamp
    - Create `NotifyTeamCommand` + handler — sends Teams notification via IGraphTeamsService, creates audit entry
    - Create `ScheduleFollowUpCommand` + handler — creates calendar event via IGraphCalendarService, creates audit entry
    - Create `GetLoanOverviewQuery` + handler, `GetLoanByIdQuery` + handler, `GetAuditQuery` + handler (chronological order, max 100 entries)
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8, 7.9_

  - [x] 5.2 Implement BuildEstate module CQRS handlers
    - Create `CreateProjectCommand` + handler — stores project with directors, returns DTO
    - Create `LaunchWorkspaceCommand` + handler — checks workspace not already launched, calls IGraphDriveService for folder structure (Planning Documents, Contracts, Site Reports, Financial)
    - Create `CreateTaskBoardCommand` + handler — calls IGraphPlannerService to create board with 3+ buckets (To Do, In Progress, Completed) and 3+ tasks
    - Create `NotifyDirectorsCommand` + handler — checks at least 1 director assigned, sends emails via IGraphMailService to all directors, returns count
    - Create `ScheduleKickoffCommand` + handler — creates calendar event within 14 days for all team members
    - Create `GetBuildEstateOverviewQuery` + handler, `GetProjectByIdQuery` + handler, `GetWeeklyReportQuery` + handler (tasks by status, milestones due within 7 days, team activity count)
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8, 8.9_

- [x] 6. Implement Application layer — CQRS handlers for CEO Dashboard and Productivity modules
  - [x] 6.1 Implement CEO Dashboard module CQRS handlers (read-only queries)
    - Create `GetCeoOverviewQuery` + handler — aggregates counts from calendar, mail, planner, drive, security services; returns partial results with error indicators on individual service failures
    - Create `GetTodayQuery` + handler — calls IGraphCalendarService.GetTodayEvents, max 50 events
    - Create `GetEmailsQuery` + handler — calls IGraphMailService.GetRecentEmails grouped by priority, max 50 summaries
    - Create `GetCalendarQuery` + handler — calls IGraphCalendarService.GetEventsForDateRange for today
    - Create `GetTasksQuery` + handler — calls IGraphPlannerService.GetPendingTasks, max 50 tasks
    - Create `GetDocumentsQuery` + handler — calls IGraphDriveService.GetRecentDocuments + GetPendingApprovals, max 50 documents
    - Create `GetSecuritySignalsQuery` + handler — calls IGraphSecurityService.GetRecentAlerts, max 50 signals
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7, 9.8_

  - [x] 6.2 Implement Productivity Assistant module CQRS handlers
    - Create `GenerateWeeklySummaryCommand` + handler — aggregates calendar, emails, tasks, documents for past 7 days; handles partial failures with error indicators
    - Create `GetProductivityOverviewQuery` + handler
    - Create `GetContextPackageQuery` + handler — returns structured JSON with calendar, emails, tasks, documents sections (all non-null)
    - Create `GetProductivityCalendarQuery` + handler — current week events, max 100
    - Create `GetProductivityEmailsQuery` + handler — email volume, top 10 senders, unread count for past 7 days
    - Create `GetProductivityTasksQuery` + handler — completed, overdue, in-progress counts for past 7 days
    - Create `GetProductivityDocumentsQuery` + handler — documents accessed/modified past 7 days, max 50
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 10.8_

- [x] 7. Checkpoint — Verify application layer compiles and all handlers are wired
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Implement Infrastructure layer — Mock Graph service implementations
  - [x] 8.1 Implement mock services for IGraphUserService, IGraphGroupService, and IGraphMailService
    - Create `MockGraphUserService` returning deterministic user profiles (Sarah Khan, Afzal Ahmed, etc.)
    - Create `MockGraphGroupService` returning department-based groups (Finance→Finance Team, HR→HR Team, etc.)
    - Create `MockGraphMailService` returning realistic email data with named senders, subjects, and bodies
    - All mock methods return non-null results with all required DTO fields populated with non-empty values
    - No external HTTP or network calls permitted
    - _Requirements: 3.2, 3.4, 14.1, 14.3_

  - [x] 8.2 Implement mock services for IGraphCalendarService, IGraphTeamsService, and IGraphDriveService
    - Create `MockGraphCalendarService` returning realistic calendar events with named attendees and business-appropriate subjects
    - Create `MockGraphTeamsService` returning channel creation results and notification confirmations
    - Create `MockGraphDriveService` returning folder structures and document lists with realistic file names and dates
    - _Requirements: 3.2, 3.4, 14.2, 14.4, 14.5_

  - [x] 8.3 Implement mock services for IGraphPlannerService, IGraphSecurityService, and IGraphReportService
    - Create `MockGraphPlannerService` returning task boards with buckets and assigned tasks
    - Create `MockGraphSecurityService` returning security alerts with severity levels and timestamps
    - Create `MockGraphReportService` returning activity report summaries
    - _Requirements: 3.2, 3.4, 14.4, 14.5_

  - [x] 8.4 Implement in-memory data stores for Demo_Mode entity persistence
    - Create `InMemoryEmployeeStore` for onboarding module with pre-seeded Sarah Khan record
    - Create `InMemoryLegalMatterStore` with pre-seeded Oakfield Estates matter
    - Create `InMemoryLoanApprovalStore` with pre-seeded Greenway Property Holdings loan
    - Create `InMemoryBuildEstateStore` with pre-seeded Riverside Heights project
    - All stores use ConcurrentDictionary for thread safety
    - _Requirements: 14.1, 14.2, 14.3, 14.4_

- [x] 9. Implement Infrastructure layer — Live Graph service implementations
  - [x] 9.1 Implement live Graph services with Microsoft Graph SDK
    - Create `LiveGraphUserService`, `LiveGraphGroupService`, `LiveGraphMailService` using Microsoft.Graph SDK
    - Create `LiveGraphCalendarService`, `LiveGraphTeamsService`, `LiveGraphDriveService` using Microsoft.Graph SDK
    - Create `LiveGraphPlannerService`, `LiveGraphSecurityService`, `LiveGraphReportService` using Microsoft.Graph SDK
    - Each implementation wraps SDK exceptions in `GraphServiceException` with operation name and failure reason
    - _Requirements: 4.2, 4.3, 4.6_

  - [x] 9.2 Implement MSAL token acquisition and caching
    - Create `TokenCacheService` using MSAL ConfidentialClientApplication
    - Implement token caching that serves cached tokens until 5 minutes before expiry
    - Configure Microsoft Entra ID settings from `GraphBridge:AzureAd` configuration section
    - Return 401 with API_Envelope when token acquisition fails
    - _Requirements: 3.3, 12.4, 12.5_

  - [x] 9.3 Create DI registration extension methods for Infrastructure layer
    - Create `AddGraphBridgeInfrastructure(IServiceCollection, IConfiguration)` extension method
    - Register mock or live implementations based on `GraphBridge:GraphMode` setting
    - Register in-memory stores as singletons for Demo_Mode
    - _Requirements: 3.6, 3.7_

- [x] 10. Implement API controllers
  - [x] 10.1 Create BaseApiController and Onboarding + Legal Matters controllers
    - Create `BaseApiController` abstract class with Mediator accessor and `[Route("api")]` prefix
    - Create `OnboardingController` with 7 endpoints matching route spec: overview, create employee, get employee, assign-groups, send-welcome-email, schedule-induction, status
    - Create `LegalMattersController` with 7 endpoints: overview, create matter, get matter, create-workspace, invite-participants, schedule-kickoff, documents
    - Each controller method contains only model binding, `mediator.Send()`, and HTTP response mapping
    - _Requirements: 1.6, 5.8, 6.9_

  - [x] 10.2 Create Loan Approvals and BuildEstate controllers
    - Create `LoanApprovalsController` with 9 endpoints: overview, create, get, generate-pack, send-customer-email, notify-team, schedule-follow-up, audit
    - Create `BuildEstateProjectsController` with 9 endpoints: overview, create, get, launch-workspace, create-task-board, notify-directors, schedule-kickoff, weekly-report
    - _Requirements: 1.6, 7.9, 8.9_

  - [x] 10.3 Create CEO Command Centre and Productivity Assistant controllers
    - Create `CeoCommandCentreController` with 8 endpoints: overview, today, emails, calendar, tasks, documents, security-signals
    - Create `ProductivityAssistantController` with 8 endpoints: overview, weekly-summary, context-package, calendar, emails, tasks, documents
    - _Requirements: 1.6, 9.8, 10.8_

- [x] 11. Checkpoint — Verify backend compiles, all endpoints respond in Demo_Mode
  - Ensure all tests pass, ask the user if questions arise.

- [x] 12. Implement Angular frontend — Core architecture and shared components
  - [x] 12.1 Create Angular 20 application with routing and core layout
    - Initialize Angular 20 project with standalone components and signals
    - Create `AppComponent` with router-outlet
    - Create `LayoutComponent` shell with sidebar navigation and top bar
    - Configure lazy-loaded routes for all six modules plus dashboard
    - Set up root URL (/) redirect to /dashboard
    - _Requirements: 11.1, 11.8_

  - [x] 12.2 Create shared UI components
    - Create `LoadingSkeletonComponent` — animated placeholder shown within 200ms of navigation
    - Create `ErrorStateComponent` — displays error category (network/validation/server) with retry button
    - Create `EmptyStateComponent` — explains what data would appear with action button
    - Create `GraphExplanationPanelComponent` — displays Graph API endpoints, permission scopes, and capability descriptions
    - _Requirements: 11.3, 11.4, 11.5, 11.6_

  - [x] 12.3 Implement HTTP interceptors and core services
    - Create `CorrelationInterceptor` — attaches `X-Correlation-ID` GUID header to every outgoing request
    - Create `LoadingInterceptor` — emits loading state signal (true on request start, false on response/error)
    - Create `ErrorInterceptor` — routes HTTP error responses to centralised `ErrorHandlerService`
    - Create `ErrorHandlerService` — centralised error state management
    - Create `ApiService` base class for typed HTTP communication with API_Envelope unwrapping
    - _Requirements: 11.7_

- [x] 13. Implement Angular frontend — Module pages
  - [x] 13.1 Create Master Dashboard and Onboarding module pages
    - Create `DashboardComponent` at /dashboard showing six module cards with: module name, business purpose description, Graph API areas used, navigation button
    - Create Onboarding feature module with: overview page, create employee form, employee detail page, action buttons for assign-groups/send-welcome-email/schedule-induction, status display
    - Wire up API service calls for all onboarding endpoints
    - _Requirements: 11.1, 11.2, 5.8_

  - [x] 13.2 Create Legal Matters and Loan Approvals module pages
    - Create Legal Matters feature module with: overview, create matter form, matter detail, workspace actions, participant management, document tree view
    - Create Loan Approvals feature module with: overview, create loan form, loan detail, generate-pack action, communication actions, audit trail view
    - Wire up API service calls for all legal matters and loan approvals endpoints
    - _Requirements: 11.1, 6.9, 7.9_

  - [x] 13.3 Create BuildEstate, CEO Dashboard, and Productivity module pages
    - Create BuildEstate feature module with: overview, create project form, project detail, workspace launch, task board view, director notifications, weekly report
    - Create CEO Command Centre feature module with: overview dashboard, today's schedule, email summary, task list, documents, security signals
    - Create Productivity Assistant feature module with: overview, weekly summary trigger, context package view, calendar/email/task/document sections
    - Wire up API service calls for all remaining module endpoints
    - _Requirements: 11.1, 8.9, 9.8, 10.8_

  - [x] 13.4 Add Graph API explanation panels to all module pages
    - Add `GraphExplanationPanelComponent` to each module page showing: endpoint names demonstrated, permission scopes required, one-sentence capability descriptions
    - Configure panel data for each module's specific Graph API integrations
    - _Requirements: 11.6_

- [x] 14. Checkpoint — Verify frontend compiles, all routes load, API calls connect to backend
  - Ensure all tests pass, ask the user if questions arise.

- [x] 15. Implement unit tests for handlers, validators, and mock services
  - [x] 15.1 Write unit tests for all FluentValidation validators
    - At least 2 tests per validator: one valid input acceptance, one invalid input rejection with correct error messages
    - Cover: CreateEmployeeRequestValidator, CreateLegalMatterRequestValidator, CreateLoanApprovalRequestValidator, CreateBuildEstateProjectRequestValidator, InviteParticipantsRequestValidator
    - _Requirements: 13.2_

  - [x] 15.2 Write unit tests for Onboarding and Legal Matter CQRS handlers
    - One test per handler verifying correct Graph service interface methods are invoked
    - Verify handlers return correctly structured DTOs
    - Verify 404 responses for non-existent entity IDs
    - Use Moq for Graph service mocking
    - _Requirements: 13.1, 5.7, 6.4_

  - [x] 15.3 Write unit tests for Loan Approval and BuildEstate CQRS handlers
    - One test per handler verifying correct Graph service interface methods are invoked
    - Verify business rule enforcement: generate-pack requires "Approved" status, send-email requires pack generated, workspace idempotency
    - Verify audit entry creation on communication actions
    - _Requirements: 13.1, 7.3, 7.5, 8.3, 8.6_

  - [x] 15.4 Write unit tests for CEO Dashboard and Productivity handlers
    - Verify partial results returned when individual Graph services fail
    - Verify response list capping at configured maximums (50 for CEO, 100 for Productivity calendar)
    - Verify context package always contains all four sections
    - _Requirements: 13.1, 9.7, 10.7_

  - [x] 15.5 Write unit tests for all mock Graph service implementations
    - One test per mock service method verifying returned DTOs have all required fields with non-null, non-empty values
    - Verify no network calls are made (mock services must not use HttpClient)
    - _Requirements: 13.3_

  - [x] 15.6 Write unit tests for middleware components
    - Test CorrelationIdMiddleware generates unique GUIDs and adds to logging scope
    - Test GlobalExceptionMiddleware maps each exception type to correct HTTP status and API_Envelope format
    - Test no stack traces or internal details leak in error responses
    - _Requirements: 13.1, 2.4, 2.5_

- [x] 16. Implement property-based tests
  - [x] 16.1 Write property test for API Envelope structure invariant
    - **Property 1: API Envelope Structure Invariant**
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.6**
    - Use FsCheck to generate random valid/invalid requests and verify all responses deserialize to valid API_Envelope

  - [x] 16.2 Write property test for CorrelationId uniqueness
    - **Property 2: CorrelationId Uniqueness**
    - **Validates: Requirements 2.5**
    - Generate sequences of requests and verify each correlationId is unique and a valid GUID

  - [x] 16.3 Write property test for validation rejection prevents handler execution
    - **Property 3: Validation Rejection Prevents Handler Execution**
    - **Validates: Requirements 1.7, 2.3, 5.2, 6.6**
    - Generate random invalid command objects and verify handlers are never invoked

  - [x] 16.4 Write property test for exception-to-status mapping
    - **Property 4: Exception-to-Status Mapping**
    - **Validates: Requirements 2.4, 2.6, 4.6, 12.5**
    - Generate random exception types and verify correct HTTP status codes and API_Envelope formatting

  - [x] 16.5 Write property test for mock services return complete data
    - **Property 5: Mock Services Return Complete Data Without Network Calls**
    - **Validates: Requirements 3.2, 3.4, 13.3**
    - Invoke all mock service methods with random parameters and verify non-null, complete DTOs returned

  - [x] 16.6 Write property test for entity creation returns unique identifier
    - **Property 6: Entity Creation Returns Unique Identifier**
    - **Validates: Requirements 5.1, 6.1, 7.1, 8.1**
    - Generate random valid creation requests and verify unique GUIDs returned

  - [x] 16.7 Write property tests for Onboarding module (Properties 7, 8, 9)
    - **Property 7: Department-Based Group Assignment** — Validates: Requirements 5.3, 5.6
    - **Property 8: Welcome Email Contains Employee Identity** — Validates: Requirements 5.4
    - **Property 9: Induction Event Duration and Attendees** — Validates: Requirements 5.5
    - Generate random department strings, name/role pairs, and employee/manager pairs

  - [x] 16.8 Write property tests for Legal Matter module (Properties 10, 11, 12, 13, 14)
    - **Property 10: Workspace Folder Structure Completeness** — Validates: Requirements 6.2, 8.2
    - **Property 11: Workspace Idempotency Guard** — Validates: Requirements 6.4, 8.3
    - **Property 12: Participant/Director Notification Count Accuracy** — Validates: Requirements 6.5, 8.5
    - **Property 13: Kickoff Scheduling Within 14-Day Window** — Validates: Requirements 6.7, 8.7
    - **Property 14: Teams Channel Named After Reference** — Validates: Requirements 6.3

  - [x] 16.9 Write property tests for Loan Approval module (Properties 15, 16, 17, 18)
    - **Property 15: Loan Operation Ordering Enforcement** — Validates: Requirements 7.3, 7.5
    - **Property 16: Communication Pack Completeness** — Validates: Requirements 7.2
    - **Property 17: Audit Trail Chronological Order and Limit** — Validates: Requirements 7.8
    - **Property 18: Audit Entry Creation on Communication Actions** — Validates: Requirements 7.4, 7.6, 7.7

  - [x] 16.10 Write property tests for BuildEstate and cross-cutting (Properties 19, 20, 21, 22, 23, 24)
    - **Property 19: Task Board Minimum Structure** — Validates: Requirements 8.4
    - **Property 20: Response List Capping** — Validates: Requirements 9.2, 9.3, 9.4, 9.5, 9.6, 10.3, 10.6
    - **Property 21: Partial Results on Service Failure** — Validates: Requirements 9.7, 10.7
    - **Property 22: Context Package Section Completeness** — Validates: Requirements 10.2
    - **Property 23: HTTP Interceptor CorrelationId Attachment** — Validates: Requirements 11.7
    - **Property 24: Token Cache Invalidation Timing** — Validates: Requirements 12.4

- [x] 17. Implement integration tests
  - [x] 17.1 Write integration tests for all module overview endpoints
    - One test per module verifying HTTP 200 + valid API_Envelope + non-null data when running in Demo_Mode
    - Use WebApplicationFactory<Program> configured with Demo_Mode
    - Verify application starts without Entra ID configuration values
    - _Requirements: 13.4, 3.5_

  - [x] 17.2 Write integration tests for validation, not-found, and workflow scenarios
    - Test validation round-trip: submit invalid request → verify 400 + field errors in envelope
    - Test not-found handling: request non-existent GUID → verify 404 + envelope
    - Test full workflow per module: create entity → trigger actions → verify status updates
    - Test correlationId propagation: verify response contains correlationId matching body
    - _Requirements: 13.4, 13.5_

- [x] 18. Checkpoint — Verify all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 19. Implement security configuration and documentation
  - [x] 19.1 Configure security and secret management
    - Create `appsettings.json` with placeholder values for Entra ID config (TenantId, ClientId, ClientSecret as "YOUR-*-HERE" strings)
    - Create `appsettings.Development.json` with Demo_Mode defaults
    - Add `appsettings.Development.json` to .gitignore
    - Verify no secrets or tokens appear in source-controlled files
    - _Requirements: 12.1, 12.2_

  - [x] 19.2 Create platform documentation
    - Create `/docs/architecture.md` with Clean Architecture layers, CQRS pattern, DI strategy, module organisation
    - Create `/docs/microsoft-graph-overview.md` explaining Graph API and how each module demonstrates specific capabilities
    - Create `/docs/authentication-and-permissions.md` with Entra ID registration, permissions table, delegated vs application permissions, admin consent, Key Vault integration
    - Create `/docs/module-guide.md` with entry per module: business scenario, endpoints, Graph areas, Angular routes
    - Create `/docs/endpoint-catalog.md` listing every endpoint with method, route, request/response examples
    - Create `README.md` with project description, architecture overview, tech stack, setup instructions, Demo_Mode explanation, module table, docs link
    - _Requirements: 12.3, 15.1, 15.2, 15.3, 15.4, 15.5, 15.6_

- [x] 20. Final checkpoint — Verify complete platform runs end-to-end in Demo_Mode
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation at logical boundaries
- Property tests validate universal correctness properties using FsCheck with minimum 100 iterations
- Unit tests validate specific examples and edge cases using xUnit + Moq + FluentAssertions
- The backend uses C# (.NET 8) and the frontend uses TypeScript (Angular 20)
- All mock services return deterministic named entities for portfolio-quality demonstrations
- Integration tests use WebApplicationFactory with Demo_Mode to avoid Graph SDK dependencies

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2", "1.3", "2.1"] },
    { "id": 2, "tasks": ["1.4", "1.5", "3.1"] },
    { "id": 3, "tasks": ["3.2", "3.3"] },
    { "id": 4, "tasks": ["4.1", "4.2"] },
    { "id": 5, "tasks": ["5.1", "5.2"] },
    { "id": 6, "tasks": ["6.1", "6.2"] },
    { "id": 7, "tasks": ["8.1", "8.2", "8.3", "8.4"] },
    { "id": 8, "tasks": ["9.1", "9.2", "9.3"] },
    { "id": 9, "tasks": ["10.1", "10.2", "10.3"] },
    { "id": 10, "tasks": ["12.1"] },
    { "id": 11, "tasks": ["12.2", "12.3"] },
    { "id": 12, "tasks": ["13.1", "13.2", "13.3"] },
    { "id": 13, "tasks": ["13.4", "19.1"] },
    { "id": 14, "tasks": ["15.1", "15.2", "15.3", "15.4", "15.5", "15.6"] },
    { "id": 15, "tasks": ["16.1", "16.2", "16.3", "16.4", "16.5", "16.6", "16.7", "16.8", "16.9", "16.10"] },
    { "id": 16, "tasks": ["17.1", "17.2"] },
    { "id": 17, "tasks": ["19.2"] }
  ]
}
```
