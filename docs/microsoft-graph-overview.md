# Microsoft Graph API Overview

## What is Microsoft Graph?

Microsoft Graph is a unified REST API that provides access to data and intelligence across Microsoft 365 services. It acts as a single endpoint (`https://graph.microsoft.com`) through which applications can interact with users, groups, mail, calendar, files, tasks, security alerts, and more — all within a single authenticated session.

Graph API eliminates the need to integrate separately with Exchange, SharePoint, Teams, Planner, and Azure AD. Instead, developers work with one consistent API surface, one authentication model (Microsoft Entra ID / MSAL), and one set of SDKs.

---

## Microsoft 365 Services Exposed via Graph

| Service | Graph API Area | Key Capabilities |
|---------|---------------|-----------------|
| Azure Active Directory | Users & Groups | User profiles, group membership, license management |
| Exchange Online | Mail | Send/receive email, manage mailboxes, mail tips |
| Exchange Online | Calendar | Create events, manage attendees, find free/busy |
| SharePoint Online | Drive (Files) | Create folders, upload documents, manage permissions |
| Microsoft Teams | Teams & Channels | Create teams/channels, post messages, send notifications |
| Microsoft Planner | Planner | Create plans, buckets, tasks, track progress |
| Microsoft Defender | Security | Security alerts, sign-in anomalies, risk detections |
| Microsoft 365 Reports | Reports | Usage analytics, activity summaries |

---

## How Each Module Demonstrates Graph API Capabilities

### Module 1: Employee Onboarding Automation

**Graph Areas:** Users, Groups, Mail, Calendar

| Capability | Graph API Used | What It Demonstrates |
|-----------|---------------|---------------------|
| Create user profile | `IGraphUserService` | User management and profile provisioning |
| Assign to groups | `IGraphGroupService` | Department-based group membership management |
| Send welcome email | `IGraphMailService` | Programmatic email composition and delivery |
| Schedule induction | `IGraphCalendarService` | Calendar event creation with multiple attendees |

**Real-World Scenario:** When HR onboards a new employee, the system automatically provisions their Microsoft 365 account, assigns them to relevant security and distribution groups, sends a personalised welcome email, and schedules their induction meeting with their manager.

---

### Module 2: Legal Matter Workspace Automation

**Graph Areas:** Drive (SharePoint), Teams, Calendar

| Capability | Graph API Used | What It Demonstrates |
|-----------|---------------|---------------------|
| Create folder structure | `IGraphDriveService` | SharePoint document library provisioning |
| Create Teams channel | `IGraphTeamsService` | Collaboration workspace creation |
| Invite participants | `IGraphTeamsService` | Team membership management |
| Schedule kickoff | `IGraphCalendarService` | Multi-attendee event scheduling |

**Real-World Scenario:** When a solicitor opens a new legal matter, the system creates a standardised SharePoint folder structure (Correspondence, Contracts, Evidence, Notes), provisions a Teams channel for case collaboration, invites internal and external participants, and schedules the matter kickoff meeting.

---

### Module 3: Loan Approval Communication Hub

**Graph Areas:** Mail, Teams, Calendar

| Capability | Graph API Used | What It Demonstrates |
|-----------|---------------|---------------------|
| Send customer email | `IGraphMailService` | Transactional email delivery with tracking |
| Notify finance team | `IGraphTeamsService` | Internal channel notifications |
| Schedule follow-up | `IGraphCalendarService` | Automated meeting scheduling |

**Real-World Scenario:** When a mortgage loan is approved, the system generates a communication pack, sends the approval notification to the customer, posts an update to the internal finance Teams channel, and schedules a customer follow-up meeting — all with full audit trail tracking.

---

### Module 4: BuildEstate Project Launch Workspace

**Graph Areas:** Drive (SharePoint), Planner, Mail, Calendar

| Capability | Graph API Used | What It Demonstrates |
|-----------|---------------|---------------------|
| Create project folders | `IGraphDriveService` | SharePoint document library with nested structure |
| Create task board | `IGraphPlannerService` | Planner plan creation with buckets and tasks |
| Notify directors | `IGraphMailService` | Bulk email notification to stakeholders |
| Schedule kickoff | `IGraphCalendarService` | Project kickoff event with team invitations |

**Real-World Scenario:** When planning approval is granted for a property development, the system creates a complete SharePoint workspace (Planning Documents, Contracts, Site Reports, Financial), provisions a Planner task board with initial project tasks, emails all assigned directors, and schedules the project kickoff meeting.

---

### Module 5: CEO Command Centre Dashboard

**Graph Areas:** Calendar, Mail, Planner, Drive, Security

| Capability | Graph API Used | What It Demonstrates |
|-----------|---------------|---------------------|
| Today's meetings | `IGraphCalendarService` | Calendar event retrieval for date range |
| Email overview | `IGraphMailService` | Email volume and priority analysis |
| Pending tasks | `IGraphPlannerService` | Task status aggregation |
| Document approvals | `IGraphDriveService` | Document activity and pending actions |
| Security signals | `IGraphSecurityService` | Threat detection and anomaly alerts |

**Real-World Scenario:** A CEO opens their command centre dashboard and sees an aggregated view of today's meetings, unread email counts by priority, pending tasks, documents awaiting approval, and any security alerts — all pulled from across Microsoft 365 in a single view. The system handles partial failures gracefully, showing available data even if one service is unavailable.

---

### Module 6: AI Meeting & Productivity Assistant

**Graph Areas:** Calendar, Mail, Planner, Drive, Reports

| Capability | Graph API Used | What It Demonstrates |
|-----------|---------------|---------------------|
| Weekly calendar | `IGraphCalendarService` | Date range event aggregation |
| Email analytics | `IGraphMailService` | Send/receive volume, top senders |
| Task summary | `IGraphPlannerService` | Completion rates and overdue tracking |
| Document activity | `IGraphDriveService` | Recent file modifications |
| Activity report | `IGraphReportService` | Usage analytics over time period |

**Real-World Scenario:** A knowledge worker requests their weekly productivity summary. The system aggregates seven days of calendar events, email volume with top senders, task completion rates, and recently accessed documents into a structured "context package" suitable for AI analysis or personal review.

---

## Graph API Permission Model

GraphBridge uses two types of permissions:

1. **Delegated Permissions** — Act on behalf of the signed-in user. Used when the application needs to access data the user has access to.

2. **Application Permissions** — Act as the application itself without a signed-in user. Used for background processes or admin-level operations.

See [Authentication and Permissions](./authentication-and-permissions.md) for the full permissions table and Entra ID configuration steps.

---

## SDK Integration Approach

GraphBridge isolates all Graph SDK usage behind abstraction interfaces:

```
Controller → MediatR → Handler → IGraph*Service → [MockImpl | LiveImpl]
```

- **Controllers** never reference Microsoft.Graph namespace
- **Handlers** depend only on interface contracts
- **Infrastructure** layer contains all SDK-specific code
- **Mock implementations** return deterministic data for Demo_Mode
- **Live implementations** use Microsoft Graph SDK with MSAL authentication

This means the entire application can run without Graph credentials (Demo_Mode) while maintaining identical business logic paths.
