---
inclusion: auto
---

# GraphBridge Enterprise Suite — Demo Mode Standards

## Purpose
The platform must run fully without real Microsoft Graph credentials. Demo mode provides realistic business data so the portfolio project is always presentable.

## Configuration
```json
{
  "GraphBridge": {
    "GraphMode": "Demo"
  }
}
```
When `GraphMode` is `Demo`, all IGraph*Service interfaces resolve to Mock implementations.
When `GraphMode` is `Live`, they resolve to real Graph SDK implementations.

## Demo Data Requirements

### Employee Onboarding
- Name: Sarah Khan
- Role: Junior Business Analyst
- Department: Finance
- Manager: Afzal Ahmed
- Groups: Finance Team, New Starters, Building Access

### Legal Matter
- Matter: Commercial Lease Review
- Client: Oakfield Estates Ltd
- Solicitor: Emma Roberts
- Type: Commercial Property

### Loan Approval
- Customer: Greenway Property Holdings
- Amount: £1,250,000
- Status: Approved
- Underwriter: James Wilson

### BuildEstate Project
- Project: Riverside Heights
- Planning Status: Approved
- Location: Reading
- Directors: 3 notified

### CEO Dashboard
- Today's meetings: 4
- Unread emails: 12
- Pending approvals: 3
- Security alerts: 1
- Active projects: 6

### Productivity Assistant
- Weekly meetings attended: 8
- Emails processed: 47
- Tasks completed: 12
- Documents accessed: 15

## Mock Service Rules
- Return data that looks real and business-appropriate
- Include realistic timestamps, GUIDs, names
- Simulate delays (50-200ms) for realistic UX
- Return proper pagination metadata
- Support filtering/sorting on mock data where feasible
- Never return empty arrays unless testing empty states

## UI Demo Mode Indicator
- Show a subtle "Demo Mode" badge in the UI header
- Tooltip: "Running without Microsoft Graph credentials. All data is simulated."
