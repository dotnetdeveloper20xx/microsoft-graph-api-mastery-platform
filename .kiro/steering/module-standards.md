---
inclusion: auto
---

# GraphBridge Enterprise Suite — Module Implementation Standards

## Every Module Must Have

### Backend
- API endpoints (RESTful, grouped by module)
- Application layer CQRS commands/queries
- DTOs (request/response, never exposing domain or Graph models)
- Validators (FluentValidation) where input exists
- Domain/service abstractions (IGraph*Service usage)
- Infrastructure Graph API implementation (real + mock)

### Frontend
- Angular feature module/page
- Dashboard cards on the module landing
- Loading states (skeleton/spinner)
- Error states (user-friendly message + retry)
- Empty states (guidance message + action)
- Graph API call explanation panels
- Clear description text showing what the module demonstrates

## Module Landing Page Pattern
Every module landing page must include:
1. **What this module does** — business scenario explanation
2. **Why Microsoft Graph matters here** — integration value
3. **Business workflow diagram** — visual stepper showing the flow
4. **Key Graph API areas used** — badges/chips showing which APIs
5. **Demo actions** — buttons to trigger workflows
6. **Results panel** — showing what happened after action

## Action Button Pattern
When a user clicks an action:
1. Show loading indicator
2. Call backend endpoint
3. Display result showing:
   - What action was triggered
   - Which backend endpoint was called
   - Which Graph API capability this represents
   - The demo result (data returned)

## Module Endpoint Patterns

### Module 1: Employee Onboarding
```
GET    /api/onboarding/overview
POST   /api/onboarding/employees
GET    /api/onboarding/employees/{id}
POST   /api/onboarding/employees/{id}/assign-groups
POST   /api/onboarding/employees/{id}/send-welcome-email
POST   /api/onboarding/employees/{id}/schedule-induction
GET    /api/onboarding/employees/{id}/status
```

### Module 2: Legal Matter Workspace
```
GET    /api/legal-matters/overview
POST   /api/legal-matters
GET    /api/legal-matters/{id}
POST   /api/legal-matters/{id}/create-workspace
POST   /api/legal-matters/{id}/invite-participants
POST   /api/legal-matters/{id}/schedule-kickoff
GET    /api/legal-matters/{id}/documents
```

### Module 3: Loan Approval Communication
```
GET    /api/loan-approvals/overview
POST   /api/loan-approvals
GET    /api/loan-approvals/{id}
POST   /api/loan-approvals/{id}/generate-pack
POST   /api/loan-approvals/{id}/send-customer-email
POST   /api/loan-approvals/{id}/notify-team
POST   /api/loan-approvals/{id}/schedule-follow-up
GET    /api/loan-approvals/{id}/audit
```

### Module 4: BuildEstate Project Launch
```
GET    /api/buildestate-projects/overview
POST   /api/buildestate-projects
GET    /api/buildestate-projects/{id}
POST   /api/buildestate-projects/{id}/launch-workspace
POST   /api/buildestate-projects/{id}/create-task-board
POST   /api/buildestate-projects/{id}/notify-directors
POST   /api/buildestate-projects/{id}/schedule-kickoff
GET    /api/buildestate-projects/{id}/weekly-report
```

### Module 5: CEO Command Centre
```
GET    /api/ceo-command-centre/overview
GET    /api/ceo-command-centre/today
GET    /api/ceo-command-centre/emails
GET    /api/ceo-command-centre/calendar
GET    /api/ceo-command-centre/tasks
GET    /api/ceo-command-centre/documents
GET    /api/ceo-command-centre/security-signals
```

### Module 6: Productivity Assistant
```
GET    /api/productivity-assistant/overview
POST   /api/productivity-assistant/weekly-summary
GET    /api/productivity-assistant/context-package
GET    /api/productivity-assistant/calendar
GET    /api/productivity-assistant/emails
GET    /api/productivity-assistant/tasks
GET    /api/productivity-assistant/documents
```
