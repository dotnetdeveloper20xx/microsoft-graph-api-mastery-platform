# Module Guide

GraphBridge Enterprise Suite comprises six self-contained business modules. Each module demonstrates enterprise-level integration between custom business workflows and Microsoft 365 services via the Microsoft Graph API.

---

## Module 1: Employee Onboarding Automation

### Business Scenario

An HR administrator needs to onboard a new employee to the organisation. Rather than manually provisioning Microsoft 365 resources, the system automates the entire process: creating the employee profile, assigning them to relevant security and distribution groups based on department, sending a personalised welcome email, and scheduling their induction meeting with their manager.

### Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/onboarding/overview` | Module overview with summary stats |
| POST | `/api/onboarding/employees` | Create new employee record |
| GET | `/api/onboarding/employees/{id}` | Get employee details by ID |
| POST | `/api/onboarding/employees/{id}/assign-groups` | Assign department-based groups |
| POST | `/api/onboarding/employees/{id}/send-welcome-email` | Send personalised welcome email |
| POST | `/api/onboarding/employees/{id}/schedule-induction` | Schedule 60-min induction meeting |
| GET | `/api/onboarding/employees/{id}/status` | Get onboarding step completion status |

### Graph API Areas

- **IGraphUserService** — User profile creation and management
- **IGraphGroupService** — Department-based group membership assignment
- **IGraphMailService** — Welcome email composition and delivery
- **IGraphCalendarService** — Induction meeting scheduling with attendees

### Angular Routes

| Route | Page |
|-------|------|
| `/onboarding` | Module overview |
| `/onboarding/create` | Create employee form |
| `/onboarding/:id` | Employee detail with actions |

---

## Module 2: Legal Matter Workspace Automation

### Business Scenario

A solicitor opens a new legal matter (e.g., Commercial Lease Review for Oakfield Estates Ltd). The system automatically provisions a complete Microsoft 365 workspace: a SharePoint folder structure with standard legal folders, a Teams channel for case collaboration, external participant invitations, and a kickoff meeting scheduled within 14 days.

### Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/legal-matters/overview` | Module overview with active matters |
| POST | `/api/legal-matters` | Create new legal matter |
| GET | `/api/legal-matters/{id}` | Get matter details by ID |
| POST | `/api/legal-matters/{id}/create-workspace` | Provision SharePoint + Teams workspace |
| POST | `/api/legal-matters/{id}/invite-participants` | Invite 1-50 participants |
| POST | `/api/legal-matters/{id}/schedule-kickoff` | Schedule kickoff meeting (within 14 days) |
| GET | `/api/legal-matters/{id}/documents` | Get document folder tree |

### Graph API Areas

- **IGraphDriveService** — SharePoint folder structure creation (Correspondence, Contracts, Evidence, Notes)
- **IGraphTeamsService** — Teams channel creation named after matter reference number
- **IGraphCalendarService** — Kickoff meeting scheduling with all participants

### Angular Routes

| Route | Page |
|-------|------|
| `/legal-matters` | Module overview |
| `/legal-matters/create` | Create matter form |
| `/legal-matters/:id` | Matter detail with workspace actions |

---

## Module 3: Loan Approval Communication Hub

### Business Scenario

A loan processor records a mortgage approval (e.g., £1,250,000 for Greenway Property Holdings). The system generates a communication pack containing customer email content, internal team notifications, and a document checklist. It then orchestrates sending the approval email to the customer, notifying the internal finance team via Teams, and scheduling a customer follow-up meeting — all with a chronological audit trail.

### Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/loan-approvals/overview` | Module overview with loan summaries |
| POST | `/api/loan-approvals` | Record new loan approval |
| GET | `/api/loan-approvals/{id}` | Get loan details by ID |
| POST | `/api/loan-approvals/{id}/generate-pack` | Generate communication pack (requires Approved status) |
| POST | `/api/loan-approvals/{id}/send-customer-email` | Send approval email to customer (requires pack) |
| POST | `/api/loan-approvals/{id}/notify-team` | Send Teams notification to finance channel |
| POST | `/api/loan-approvals/{id}/schedule-follow-up` | Schedule customer follow-up meeting |
| GET | `/api/loan-approvals/{id}/audit` | Get chronological audit trail (max 100 entries) |

### Graph API Areas

- **IGraphMailService** — Customer approval email delivery with audit tracking
- **IGraphTeamsService** — Internal finance channel notifications
- **IGraphCalendarService** — Follow-up meeting scheduling

### Angular Routes

| Route | Page |
|-------|------|
| `/loan-approvals` | Module overview |
| `/loan-approvals/create` | Create loan approval form |
| `/loan-approvals/:id` | Loan detail with communication actions |

---

## Module 4: BuildEstate Project Launch Workspace

### Business Scenario

A project manager receives planning approval for a property development (e.g., "Riverside Heights" in Reading). The system provisions a complete project workspace: SharePoint folder structure for project documentation, a Planner task board with initial tasks distributed across buckets, email notifications to all assigned directors, and a project kickoff meeting scheduled for the entire team.

### Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/buildestate-projects/overview` | Module overview with project summaries |
| POST | `/api/buildestate-projects` | Record new project with directors |
| GET | `/api/buildestate-projects/{id}` | Get project details by ID |
| POST | `/api/buildestate-projects/{id}/launch-workspace` | Create SharePoint folder structure |
| POST | `/api/buildestate-projects/{id}/create-task-board` | Create Planner board with 3+ buckets |
| POST | `/api/buildestate-projects/{id}/notify-directors` | Email all assigned directors |
| POST | `/api/buildestate-projects/{id}/schedule-kickoff` | Schedule project kickoff (within 14 days) |
| GET | `/api/buildestate-projects/{id}/weekly-report` | Get weekly progress report |

### Graph API Areas

- **IGraphDriveService** — SharePoint project folders (Planning Documents, Contracts, Site Reports, Financial)
- **IGraphPlannerService** — Task board with To Do, In Progress, Completed buckets
- **IGraphMailService** — Director notification emails (1-20 directors)
- **IGraphCalendarService** — Project kickoff event for all team members

### Angular Routes

| Route | Page |
|-------|------|
| `/buildestate-projects` | Module overview |
| `/buildestate-projects/create` | Create project form |
| `/buildestate-projects/:id` | Project detail with workspace actions |

---

## Module 5: CEO Command Centre Dashboard

### Business Scenario

A CEO opens their executive command centre to get a real-time overview of their Microsoft 365 universe. The dashboard aggregates today's meetings, unread emails by priority, pending tasks, documents awaiting approval, and active security signals — all in a single view. If any individual data source is unavailable, the dashboard shows partial results with error indicators rather than failing entirely.

### Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/ceo-command-centre/overview` | Aggregated counts across all services |
| GET | `/api/ceo-command-centre/today` | Today's schedule (max 50 events) |
| GET | `/api/ceo-command-centre/emails` | Emails by priority (max 50 summaries) |
| GET | `/api/ceo-command-centre/calendar` | Calendar events for today |
| GET | `/api/ceo-command-centre/tasks` | Pending tasks (max 50) |
| GET | `/api/ceo-command-centre/documents` | Recent/pending documents (max 50) |
| GET | `/api/ceo-command-centre/security-signals` | Security alerts (max 50 signals) |

### Graph API Areas

- **IGraphCalendarService** — Today's calendar events
- **IGraphMailService** — Email volume and priority grouping
- **IGraphPlannerService** — Pending task status aggregation
- **IGraphDriveService** — Document activity and pending approvals
- **IGraphSecurityService** — Security alerts and sign-in anomalies

### Angular Routes

| Route | Page |
|-------|------|
| `/ceo-command-centre` | Executive dashboard overview |
| `/ceo-command-centre/today` | Today's schedule detail |
| `/ceo-command-centre/emails` | Email analytics |
| `/ceo-command-centre/security` | Security signals |

---

## Module 6: AI Meeting & Productivity Assistant

### Business Scenario

A knowledge worker requests their weekly productivity summary. The system aggregates seven days of calendar events, email volume with top senders, task completion rates, and recently accessed documents into a structured "context package." This JSON-structured output is designed to be fed into AI models for analysis, recommendations, or natural language summaries.

### Backend Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/productivity-assistant/overview` | Module overview with quick stats |
| POST | `/api/productivity-assistant/weekly-summary` | Generate 7-day productivity summary |
| GET | `/api/productivity-assistant/context-package` | Structured AI-ready context data |
| GET | `/api/productivity-assistant/calendar` | This week's events (max 100) |
| GET | `/api/productivity-assistant/emails` | Email volume, top senders, unread count |
| GET | `/api/productivity-assistant/tasks` | Task completion metrics (7 days) |
| GET | `/api/productivity-assistant/documents` | Recently modified documents (max 50) |

### Graph API Areas

- **IGraphCalendarService** — Weekly calendar event aggregation
- **IGraphMailService** — Send/receive volume, top 10 senders, unread count
- **IGraphPlannerService** — Completed, overdue, and in-progress task counts
- **IGraphDriveService** — Documents accessed or modified in past 7 days
- **IGraphReportService** — Activity and usage analytics

### Angular Routes

| Route | Page |
|-------|------|
| `/productivity-assistant` | Module overview |
| `/productivity-assistant/summary` | Weekly summary view |
| `/productivity-assistant/context` | AI context package viewer |
