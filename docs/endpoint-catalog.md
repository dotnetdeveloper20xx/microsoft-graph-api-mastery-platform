# Endpoint Catalog

All API endpoints return responses wrapped in the standard `ApiEnvelope<T>` format.

## Response Envelope Format

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { },
  "errors": [],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

---

## Employee Onboarding Module

### GET /api/onboarding/overview

Returns module overview with summary statistics.

**Response:**
```json
{
  "success": true,
  "message": "Onboarding overview retrieved",
  "data": {
    "totalEmployees": 5,
    "pendingOnboarding": 2,
    "completedOnboarding": 3
  },
  "errors": [],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### POST /api/onboarding/employees

Creates a new employee onboarding record.

**Request:**
```json
{
  "name": "Sarah Khan",
  "role": "Junior Business Analyst",
  "department": "Finance",
  "managerName": "Afzal Ahmed",
  "email": "sarah.khan@company.com"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Employee created successfully",
  "data": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "name": "Sarah Khan",
    "role": "Junior Business Analyst",
    "department": "Finance",
    "managerName": "Afzal Ahmed",
    "email": "sarah.khan@company.com",
    "status": {
      "profileCreated": true,
      "groupsAssigned": false,
      "welcomeEmailSent": false,
      "inductionScheduled": false
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "b2c3d4e5-f6a7-8901-bcde-f23456789012"
}
```

### GET /api/onboarding/employees/{id}

Returns employee details by ID.

**Response:**
```json
{
  "success": true,
  "message": "Employee retrieved",
  "data": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "name": "Sarah Khan",
    "role": "Junior Business Analyst",
    "department": "Finance",
    "managerName": "Afzal Ahmed",
    "email": "sarah.khan@company.com",
    "status": {
      "profileCreated": true,
      "groupsAssigned": true,
      "welcomeEmailSent": true,
      "inductionScheduled": true
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "c3d4e5f6-a7b8-9012-cdef-345678901234"
}
```

### POST /api/onboarding/employees/{id}/assign-groups

Assigns the employee to department-based Microsoft 365 groups.

**Response:**
```json
{
  "success": true,
  "message": "Groups assigned successfully",
  "data": {
    "employeeId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "groupsAssigned": ["Finance Team", "All Staff", "UK Office"]
  },
  "errors": [],
  "timestamp": "2024-01-15T10:31:00.000Z",
  "correlationId": "d4e5f6a7-b8c9-0123-defa-456789012345"
}
```

### POST /api/onboarding/employees/{id}/send-welcome-email

Sends a personalised welcome email to the employee.

**Response:**
```json
{
  "success": true,
  "message": "Welcome email sent successfully",
  "data": {
    "employeeId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "emailSentTo": "sarah.khan@company.com",
    "sentAt": "2024-01-15T10:32:00.000Z"
  },
  "errors": [],
  "timestamp": "2024-01-15T10:32:00.000Z",
  "correlationId": "e5f6a7b8-c9d0-1234-efab-567890123456"
}
```

### POST /api/onboarding/employees/{id}/schedule-induction

Schedules a 60-minute induction meeting with the employee and their manager.

**Response:**
```json
{
  "success": true,
  "message": "Induction scheduled successfully",
  "data": {
    "employeeId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "event": {
      "subject": "Induction: Sarah Khan",
      "start": "2024-01-17T09:00:00.000Z",
      "end": "2024-01-17T10:00:00.000Z",
      "attendees": ["sarah.khan@company.com", "afzal.ahmed@company.com"]
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T10:33:00.000Z",
  "correlationId": "f6a7b8c9-d0e1-2345-fabc-678901234567"
}
```

### GET /api/onboarding/employees/{id}/status

Returns the onboarding step completion status for an employee.

**Response:**
```json
{
  "success": true,
  "message": "Onboarding status retrieved",
  "data": {
    "profileCreated": true,
    "groupsAssigned": true,
    "welcomeEmailSent": false,
    "inductionScheduled": false
  },
  "errors": [],
  "timestamp": "2024-01-15T10:34:00.000Z",
  "correlationId": "a7b8c9d0-e1f2-3456-abcd-789012345678"
}
```

---

## Legal Matter Workspace Module

### GET /api/legal-matters/overview

Returns module overview with active legal matters.

**Response:**
```json
{
  "success": true,
  "message": "Legal matters overview retrieved",
  "data": {
    "totalMatters": 8,
    "activeMatters": 5,
    "workspacesCreated": 4
  },
  "errors": [],
  "timestamp": "2024-01-15T10:35:00.000Z",
  "correlationId": "b8c9d0e1-f2a3-4567-bcde-890123456789"
}
```

### POST /api/legal-matters

Creates a new legal matter.

**Request:**
```json
{
  "clientName": "Oakfield Estates Ltd",
  "matterType": "Commercial Lease Review",
  "assignedSolicitor": "Emma Roberts"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Legal matter created successfully",
  "data": {
    "id": "c8d9e0f1-a2b3-4567-cdef-901234567890",
    "referenceNumber": "LM-2024-001",
    "clientName": "Oakfield Estates Ltd",
    "matterType": "Commercial Lease Review",
    "assignedSolicitor": "Emma Roberts",
    "workspaceCreated": false,
    "participantCount": 0
  },
  "errors": [],
  "timestamp": "2024-01-15T10:36:00.000Z",
  "correlationId": "c9d0e1f2-a3b4-5678-cdef-012345678901"
}
```

### GET /api/legal-matters/{id}

Returns legal matter details by ID.

**Response:**
```json
{
  "success": true,
  "message": "Legal matter retrieved",
  "data": {
    "id": "c8d9e0f1-a2b3-4567-cdef-901234567890",
    "referenceNumber": "LM-2024-001",
    "clientName": "Oakfield Estates Ltd",
    "matterType": "Commercial Lease Review",
    "assignedSolicitor": "Emma Roberts",
    "workspaceCreated": true,
    "participantCount": 3
  },
  "errors": [],
  "timestamp": "2024-01-15T10:37:00.000Z",
  "correlationId": "d0e1f2a3-b4c5-6789-defa-123456789012"
}
```

### POST /api/legal-matters/{id}/create-workspace

Provisions SharePoint folder structure and Teams channel for the matter.

**Response:**
```json
{
  "success": true,
  "message": "Workspace created successfully",
  "data": {
    "matterId": "c8d9e0f1-a2b3-4567-cdef-901234567890",
    "folderStructure": {
      "folderName": "LM-2024-001",
      "children": [
        { "folderName": "Correspondence", "children": [] },
        { "folderName": "Contracts", "children": [] },
        { "folderName": "Evidence", "children": [] },
        { "folderName": "Notes", "children": [] }
      ]
    },
    "teamsChannel": "LM-2024-001"
  },
  "errors": [],
  "timestamp": "2024-01-15T10:38:00.000Z",
  "correlationId": "e1f2a3b4-c5d6-7890-efab-234567890123"
}
```

### POST /api/legal-matters/{id}/invite-participants

Invites participants to the matter workspace.

**Request:**
```json
{
  "participants": [
    "john.smith@external-firm.com",
    "jane.doe@company.com",
    "expert.witness@consultancy.com"
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Participants invited successfully",
  "data": {
    "matterId": "c8d9e0f1-a2b3-4567-cdef-901234567890",
    "invitedCount": 3
  },
  "errors": [],
  "timestamp": "2024-01-15T10:39:00.000Z",
  "correlationId": "f2a3b4c5-d6e7-8901-fabc-345678901234"
}
```

### POST /api/legal-matters/{id}/schedule-kickoff

Schedules the matter kickoff meeting within 14 days.

**Response:**
```json
{
  "success": true,
  "message": "Kickoff meeting scheduled",
  "data": {
    "matterId": "c8d9e0f1-a2b3-4567-cdef-901234567890",
    "event": {
      "subject": "Matter Kickoff: LM-2024-001",
      "start": "2024-01-22T14:00:00.000Z",
      "end": "2024-01-22T15:00:00.000Z",
      "attendees": ["emma.roberts@company.com", "john.smith@external-firm.com", "jane.doe@company.com"]
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T10:40:00.000Z",
  "correlationId": "a3b4c5d6-e7f8-9012-abcd-456789012345"
}
```

### GET /api/legal-matters/{id}/documents

Returns the document folder tree for the matter workspace.

**Response:**
```json
{
  "success": true,
  "message": "Documents retrieved",
  "data": {
    "folderName": "LM-2024-001",
    "children": [
      { "folderName": "Correspondence", "children": [] },
      { "folderName": "Contracts", "children": [
        { "folderName": "Drafts", "children": [] }
      ]},
      { "folderName": "Evidence", "children": [] },
      { "folderName": "Notes", "children": [] }
    ]
  },
  "errors": [],
  "timestamp": "2024-01-15T10:41:00.000Z",
  "correlationId": "b4c5d6e7-f8a9-0123-bcde-567890123456"
}
```

---

## Loan Approval Communication Hub

### GET /api/loan-approvals/overview

Returns module overview with loan summaries.

**Response:**
```json
{
  "success": true,
  "message": "Loan approvals overview retrieved",
  "data": {
    "totalLoans": 12,
    "approvedLoans": 8,
    "pendingCommunication": 3
  },
  "errors": [],
  "timestamp": "2024-01-15T11:00:00.000Z",
  "correlationId": "c5d6e7f8-a9b0-1234-cdef-678901234567"
}
```

### POST /api/loan-approvals

Records a new loan approval.

**Request:**
```json
{
  "customerName": "Greenway Property Holdings",
  "amount": 1250000.00,
  "propertyReference": "GW-PROP-2024-001",
  "status": "Approved"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Loan approval recorded",
  "data": {
    "id": "d6e7f8a9-b0c1-2345-defa-789012345678",
    "customerName": "Greenway Property Holdings",
    "amount": 1250000.00,
    "propertyReference": "GW-PROP-2024-001",
    "status": "Approved",
    "packGenerated": false
  },
  "errors": [],
  "timestamp": "2024-01-15T11:01:00.000Z",
  "correlationId": "d6e7f8a9-b0c1-2345-defa-789012345678"
}
```

### GET /api/loan-approvals/{id}

Returns loan approval details by ID.

**Response:**
```json
{
  "success": true,
  "message": "Loan approval retrieved",
  "data": {
    "id": "d6e7f8a9-b0c1-2345-defa-789012345678",
    "customerName": "Greenway Property Holdings",
    "amount": 1250000.00,
    "propertyReference": "GW-PROP-2024-001",
    "status": "Approved",
    "packGenerated": true
  },
  "errors": [],
  "timestamp": "2024-01-15T11:02:00.000Z",
  "correlationId": "e7f8a9b0-c1d2-3456-efab-890123456789"
}
```

### POST /api/loan-approvals/{id}/generate-pack

Generates the communication pack. Requires loan status to be "Approved".

**Response:**
```json
{
  "success": true,
  "message": "Communication pack generated",
  "data": {
    "customerEmail": {
      "subject": "Loan Approval Confirmation - GW-PROP-2024-001",
      "body": "Dear Greenway Property Holdings, We are pleased to confirm your loan of £1,250,000 has been approved..."
    },
    "internalNotificationContent": "Loan GW-PROP-2024-001 approved for Greenway Property Holdings (£1,250,000). Communication pack ready.",
    "documentChecklist": [
      "Signed loan agreement",
      "Property valuation report",
      "Insurance confirmation",
      "Direct debit mandate"
    ]
  },
  "errors": [],
  "timestamp": "2024-01-15T11:03:00.000Z",
  "correlationId": "f8a9b0c1-d2e3-4567-fabc-901234567890"
}
```

### POST /api/loan-approvals/{id}/send-customer-email

Sends the approval email to the customer. Requires communication pack to be generated.

**Response:**
```json
{
  "success": true,
  "message": "Customer email sent successfully",
  "data": {
    "loanId": "d6e7f8a9-b0c1-2345-defa-789012345678",
    "sentTo": "finance@greenway-holdings.co.uk",
    "sentAt": "2024-01-15T11:04:00.000Z"
  },
  "errors": [],
  "timestamp": "2024-01-15T11:04:00.000Z",
  "correlationId": "a9b0c1d2-e3f4-5678-abcd-012345678901"
}
```

### POST /api/loan-approvals/{id}/notify-team

Sends a Teams notification to the internal finance channel.

**Response:**
```json
{
  "success": true,
  "message": "Team notified successfully",
  "data": {
    "loanId": "d6e7f8a9-b0c1-2345-defa-789012345678",
    "channelNotified": "Finance Approvals",
    "notifiedAt": "2024-01-15T11:05:00.000Z"
  },
  "errors": [],
  "timestamp": "2024-01-15T11:05:00.000Z",
  "correlationId": "b0c1d2e3-f4a5-6789-bcde-123456789012"
}
```

### POST /api/loan-approvals/{id}/schedule-follow-up

Schedules a customer follow-up meeting.

**Response:**
```json
{
  "success": true,
  "message": "Follow-up meeting scheduled",
  "data": {
    "loanId": "d6e7f8a9-b0c1-2345-defa-789012345678",
    "event": {
      "subject": "Loan Follow-up: Greenway Property Holdings",
      "start": "2024-01-22T10:00:00.000Z",
      "end": "2024-01-22T10:30:00.000Z",
      "attendees": ["finance@greenway-holdings.co.uk", "loan.officer@company.com"]
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T11:06:00.000Z",
  "correlationId": "c1d2e3f4-a5b6-7890-cdef-234567890123"
}
```

### GET /api/loan-approvals/{id}/audit

Returns the chronological audit trail (maximum 100 entries).

**Response:**
```json
{
  "success": true,
  "message": "Audit trail retrieved",
  "data": [
    { "actionType": "PackGenerated", "timestamp": "2024-01-15T11:03:00.000Z", "status": "Completed" },
    { "actionType": "CustomerEmailSent", "timestamp": "2024-01-15T11:04:00.000Z", "status": "Completed" },
    { "actionType": "TeamNotified", "timestamp": "2024-01-15T11:05:00.000Z", "status": "Completed" },
    { "actionType": "FollowUpScheduled", "timestamp": "2024-01-15T11:06:00.000Z", "status": "Completed" }
  ],
  "errors": [],
  "timestamp": "2024-01-15T11:07:00.000Z",
  "correlationId": "d2e3f4a5-b6c7-8901-defa-345678901234"
}
```

---

## BuildEstate Project Launch Workspace

### GET /api/buildestate-projects/overview

Returns module overview with project summaries.

**Response:**
```json
{
  "success": true,
  "message": "BuildEstate projects overview retrieved",
  "data": {
    "totalProjects": 6,
    "activeProjects": 4,
    "workspacesLaunched": 3
  },
  "errors": [],
  "timestamp": "2024-01-15T12:00:00.000Z",
  "correlationId": "e3f4a5b6-c7d8-9012-efab-456789012345"
}
```

### POST /api/buildestate-projects

Records a new BuildEstate project.

**Request:**
```json
{
  "name": "Riverside Heights",
  "location": "Reading",
  "planningStatus": "Approved",
  "directors": ["James Morrison", "Claire Bennett"]
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "id": "f4a5b6c7-d8e9-0123-fabc-567890123456",
    "name": "Riverside Heights",
    "location": "Reading",
    "planningStatus": "Approved",
    "directors": ["James Morrison", "Claire Bennett"],
    "workspaceLaunched": false,
    "taskBoardCreated": false
  },
  "errors": [],
  "timestamp": "2024-01-15T12:01:00.000Z",
  "correlationId": "f4a5b6c7-d8e9-0123-fabc-567890123456"
}
```

### GET /api/buildestate-projects/{id}

Returns project details by ID.

**Response:**
```json
{
  "success": true,
  "message": "Project retrieved",
  "data": {
    "id": "f4a5b6c7-d8e9-0123-fabc-567890123456",
    "name": "Riverside Heights",
    "location": "Reading",
    "planningStatus": "Approved",
    "directors": ["James Morrison", "Claire Bennett"],
    "workspaceLaunched": true,
    "taskBoardCreated": true
  },
  "errors": [],
  "timestamp": "2024-01-15T12:02:00.000Z",
  "correlationId": "a5b6c7d8-e9f0-1234-abcd-678901234567"
}
```

### POST /api/buildestate-projects/{id}/launch-workspace

Creates a SharePoint folder structure for the project.

**Response:**
```json
{
  "success": true,
  "message": "Workspace launched successfully",
  "data": {
    "projectId": "f4a5b6c7-d8e9-0123-fabc-567890123456",
    "folderStructure": {
      "folderName": "Riverside Heights",
      "children": [
        { "folderName": "Planning Documents", "children": [] },
        { "folderName": "Contracts", "children": [] },
        { "folderName": "Site Reports", "children": [] },
        { "folderName": "Financial", "children": [] }
      ]
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T12:03:00.000Z",
  "correlationId": "b6c7d8e9-f0a1-2345-bcde-789012345678"
}
```

### POST /api/buildestate-projects/{id}/create-task-board

Creates a Planner task board with default buckets and initial tasks.

**Response:**
```json
{
  "success": true,
  "message": "Task board created successfully",
  "data": {
    "buckets": [
      {
        "name": "To Do",
        "tasks": [
          { "title": "Complete site survey", "status": "NotStarted", "assignedTo": "James Morrison" },
          { "title": "Submit environmental report", "status": "NotStarted", "assignedTo": "Claire Bennett" }
        ]
      },
      {
        "name": "In Progress",
        "tasks": [
          { "title": "Finalise contractor agreements", "status": "InProgress", "assignedTo": "James Morrison" }
        ]
      },
      {
        "name": "Completed",
        "tasks": []
      }
    ]
  },
  "errors": [],
  "timestamp": "2024-01-15T12:04:00.000Z",
  "correlationId": "c7d8e9f0-a1b2-3456-cdef-890123456789"
}
```

### POST /api/buildestate-projects/{id}/notify-directors

Sends notification emails to all assigned directors.

**Response:**
```json
{
  "success": true,
  "message": "Directors notified successfully",
  "data": {
    "projectId": "f4a5b6c7-d8e9-0123-fabc-567890123456",
    "notificationsSent": 2
  },
  "errors": [],
  "timestamp": "2024-01-15T12:05:00.000Z",
  "correlationId": "d8e9f0a1-b2c3-4567-defa-901234567890"
}
```

### POST /api/buildestate-projects/{id}/schedule-kickoff

Schedules the project kickoff meeting within 14 days.

**Response:**
```json
{
  "success": true,
  "message": "Kickoff meeting scheduled",
  "data": {
    "projectId": "f4a5b6c7-d8e9-0123-fabc-567890123456",
    "event": {
      "subject": "Project Kickoff: Riverside Heights",
      "start": "2024-01-25T09:00:00.000Z",
      "end": "2024-01-25T10:30:00.000Z",
      "attendees": ["james.morrison@company.com", "claire.bennett@company.com"]
    }
  },
  "errors": [],
  "timestamp": "2024-01-15T12:06:00.000Z",
  "correlationId": "e9f0a1b2-c3d4-5678-efab-012345678901"
}
```

### GET /api/buildestate-projects/{id}/weekly-report

Returns the weekly progress report.

**Response:**
```json
{
  "success": true,
  "message": "Weekly report generated",
  "data": {
    "tasksToDo": 4,
    "tasksInProgress": 3,
    "tasksCompleted": 2,
    "milestonesDueThisWeek": [
      "Complete site survey",
      "Submit environmental report"
    ],
    "teamActivityCount": 15
  },
  "errors": [],
  "timestamp": "2024-01-15T12:07:00.000Z",
  "correlationId": "f0a1b2c3-d4e5-6789-fabc-123456789012"
}
```

---

## CEO Command Centre Dashboard

### GET /api/ceo-command-centre/overview

Returns aggregated counts across all Microsoft 365 services.

**Response:**
```json
{
  "success": true,
  "message": "CEO command centre overview retrieved",
  "data": {
    "todayMeetingsCount": 5,
    "unreadEmailsCount": 12,
    "pendingTasksCount": 8,
    "pendingDocumentApprovalsCount": 3,
    "activeSecuritySignalsCount": 2,
    "unavailableSections": []
  },
  "errors": [],
  "timestamp": "2024-01-15T13:00:00.000Z",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-234567890123"
}
```

### GET /api/ceo-command-centre/today

Returns today's calendar events (maximum 50).

**Response:**
```json
{
  "success": true,
  "message": "Today's schedule retrieved",
  "data": [
    {
      "subject": "Board Meeting",
      "start": "2024-01-15T09:00:00.000Z",
      "end": "2024-01-15T10:30:00.000Z",
      "attendees": ["ceo@company.com", "cfo@company.com", "cto@company.com"]
    },
    {
      "subject": "Investor Call",
      "start": "2024-01-15T14:00:00.000Z",
      "end": "2024-01-15T15:00:00.000Z",
      "attendees": ["ceo@company.com", "ir@company.com"]
    }
  ],
  "errors": [],
  "timestamp": "2024-01-15T13:01:00.000Z",
  "correlationId": "b2c3d4e5-f6a7-8901-bcde-345678901234"
}
```

### GET /api/ceo-command-centre/emails

Returns emails grouped by priority (maximum 50 summaries).

**Response:**
```json
{
  "success": true,
  "message": "Email summary retrieved",
  "data": [
    { "subject": "Q4 Financial Results", "from": "cfo@company.com", "priority": "High", "receivedAt": "2024-01-15T08:30:00.000Z", "isRead": false },
    { "subject": "Team Standup Notes", "from": "pm@company.com", "priority": "Normal", "receivedAt": "2024-01-15T09:00:00.000Z", "isRead": true },
    { "subject": "Newsletter", "from": "marketing@external.com", "priority": "Low", "receivedAt": "2024-01-15T07:00:00.000Z", "isRead": false }
  ],
  "errors": [],
  "timestamp": "2024-01-15T13:02:00.000Z",
  "correlationId": "c3d4e5f6-a7b8-9012-cdef-456789012345"
}
```

### GET /api/ceo-command-centre/calendar

Returns calendar events for today.

**Response:**
```json
{
  "success": true,
  "message": "Calendar events retrieved",
  "data": [
    {
      "subject": "Board Meeting",
      "start": "2024-01-15T09:00:00.000Z",
      "end": "2024-01-15T10:30:00.000Z",
      "attendees": ["ceo@company.com", "cfo@company.com"]
    }
  ],
  "errors": [],
  "timestamp": "2024-01-15T13:03:00.000Z",
  "correlationId": "d4e5f6a7-b8c9-0123-defa-567890123456"
}
```

### GET /api/ceo-command-centre/tasks

Returns pending tasks with status (maximum 50).

**Response:**
```json
{
  "success": true,
  "message": "Pending tasks retrieved",
  "data": [
    { "title": "Review Q4 budget proposal", "status": "InProgress", "dueDate": "2024-01-16T17:00:00.000Z" },
    { "title": "Approve vendor contracts", "status": "NotStarted", "dueDate": "2024-01-18T17:00:00.000Z" },
    { "title": "Sign off marketing campaign", "status": "Overdue", "dueDate": "2024-01-14T17:00:00.000Z" }
  ],
  "errors": [],
  "timestamp": "2024-01-15T13:04:00.000Z",
  "correlationId": "e5f6a7b8-c9d0-1234-efab-678901234567"
}
```

### GET /api/ceo-command-centre/documents

Returns documents pending approval or recently modified (maximum 50).

**Response:**
```json
{
  "success": true,
  "message": "Documents retrieved",
  "data": [
    { "name": "Q4 Financial Report.xlsx", "modifiedBy": "Sarah Johnson", "modifiedAt": "2024-01-15T08:00:00.000Z", "location": "Finance/Reports" },
    { "name": "Board Presentation.pptx", "modifiedBy": "Tom Harris", "modifiedAt": "2024-01-14T16:30:00.000Z", "location": "Executive/Presentations" }
  ],
  "errors": [],
  "timestamp": "2024-01-15T13:05:00.000Z",
  "correlationId": "f6a7b8c9-d0e1-2345-fabc-789012345678"
}
```

### GET /api/ceo-command-centre/security-signals

Returns security alerts and sign-in anomalies (maximum 50).

**Response:**
```json
{
  "success": true,
  "message": "Security signals retrieved",
  "data": [
    {
      "title": "Unusual sign-in activity",
      "severity": "Medium",
      "detectedAt": "2024-01-15T06:30:00.000Z",
      "description": "Sign-in from unfamiliar location detected for user j.smith@company.com"
    },
    {
      "title": "Multiple failed authentication attempts",
      "severity": "High",
      "detectedAt": "2024-01-15T04:15:00.000Z",
      "description": "15 failed login attempts detected for service account svc-reports@company.com"
    }
  ],
  "errors": [],
  "timestamp": "2024-01-15T13:06:00.000Z",
  "correlationId": "a7b8c9d0-e1f2-3456-abcd-890123456789"
}
```

---

## AI Meeting & Productivity Assistant

### GET /api/productivity-assistant/overview

Returns module overview with quick stats.

**Response:**
```json
{
  "success": true,
  "message": "Productivity assistant overview retrieved",
  "data": {
    "weeklyMeetings": 12,
    "emailsProcessed": 87,
    "tasksCompleted": 5,
    "documentsAccessed": 14
  },
  "errors": [],
  "timestamp": "2024-01-15T14:00:00.000Z",
  "correlationId": "b8c9d0e1-f2a3-4567-bcde-901234567890"
}
```

### POST /api/productivity-assistant/weekly-summary

Generates the 7-day productivity summary.

**Response:**
```json
{
  "success": true,
  "message": "Weekly summary generated",
  "data": {
    "weeklyEvents": [
      { "subject": "Sprint Planning", "start": "2024-01-08T10:00:00.000Z", "end": "2024-01-08T11:00:00.000Z", "attendees": ["team@company.com"] },
      { "subject": "Client Review", "start": "2024-01-10T14:00:00.000Z", "end": "2024-01-10T15:00:00.000Z", "attendees": ["client@external.com"] }
    ],
    "emailSummary": {
      "totalSent": 34,
      "totalReceived": 87,
      "unreadCount": 12,
      "topSenders": [
        { "senderName": "Project Team", "messageCount": 15 },
        { "senderName": "Sarah Khan", "messageCount": 8 }
      ]
    },
    "taskSummary": { "completed": 5, "overdue": 2, "inProgress": 4 },
    "recentDocuments": [
      { "name": "Sprint Report.docx", "modifiedBy": "Afzal Ahmed", "modifiedAt": "2024-01-14T16:00:00.000Z", "location": "Team/Reports" }
    ],
    "unavailableSections": []
  },
  "errors": [],
  "timestamp": "2024-01-15T14:01:00.000Z",
  "correlationId": "c9d0e1f2-a3b4-5678-cdef-012345678901"
}
```

### GET /api/productivity-assistant/context-package

Returns structured AI-ready context data with all sections.

**Response:**
```json
{
  "success": true,
  "message": "Context package retrieved",
  "data": {
    "calendar": [
      { "subject": "Sprint Planning", "start": "2024-01-08T10:00:00.000Z", "end": "2024-01-08T11:00:00.000Z" }
    ],
    "emails": {
      "totalSent": 34,
      "totalReceived": 87,
      "unreadCount": 12,
      "topSenders": [{ "senderName": "Project Team", "messageCount": 15 }]
    },
    "tasks": { "completed": 5, "overdue": 2, "inProgress": 4 },
    "documents": [
      { "name": "Sprint Report.docx", "modifiedBy": "Afzal Ahmed", "modifiedAt": "2024-01-14T16:00:00.000Z", "location": "Team/Reports" }
    ]
  },
  "errors": [],
  "timestamp": "2024-01-15T14:02:00.000Z",
  "correlationId": "d0e1f2a3-b4c5-6789-defa-123456789012"
}
```

### GET /api/productivity-assistant/calendar

Returns this week's calendar events (maximum 100).

**Response:**
```json
{
  "success": true,
  "message": "Weekly calendar retrieved",
  "data": [
    { "subject": "Sprint Planning", "start": "2024-01-15T10:00:00.000Z", "end": "2024-01-15T11:00:00.000Z", "attendees": ["team@company.com"] },
    { "subject": "1:1 with Manager", "start": "2024-01-16T09:00:00.000Z", "end": "2024-01-16T09:30:00.000Z", "attendees": ["manager@company.com"] },
    { "subject": "Client Demo", "start": "2024-01-17T14:00:00.000Z", "end": "2024-01-17T15:00:00.000Z", "attendees": ["client@external.com"] }
  ],
  "errors": [],
  "timestamp": "2024-01-15T14:03:00.000Z",
  "correlationId": "e1f2a3b4-c5d6-7890-efab-234567890123"
}
```

### GET /api/productivity-assistant/emails

Returns email volume, top senders, and unread count for past 7 days.

**Response:**
```json
{
  "success": true,
  "message": "Email analytics retrieved",
  "data": {
    "totalSent": 34,
    "totalReceived": 87,
    "unreadCount": 12,
    "topSenders": [
      { "senderName": "Project Team", "messageCount": 15 },
      { "senderName": "Sarah Khan", "messageCount": 8 },
      { "senderName": "HR Notifications", "messageCount": 7 },
      { "senderName": "James Morrison", "messageCount": 6 },
      { "senderName": "Finance Dept", "messageCount": 5 }
    ]
  },
  "errors": [],
  "timestamp": "2024-01-15T14:04:00.000Z",
  "correlationId": "f2a3b4c5-d6e7-8901-fabc-345678901234"
}
```

### GET /api/productivity-assistant/tasks

Returns task completion metrics for past 7 days.

**Response:**
```json
{
  "success": true,
  "message": "Task summary retrieved",
  "data": {
    "completed": 5,
    "overdue": 2,
    "inProgress": 4
  },
  "errors": [],
  "timestamp": "2024-01-15T14:05:00.000Z",
  "correlationId": "a3b4c5d6-e7f8-9012-abcd-456789012345"
}
```

### GET /api/productivity-assistant/documents

Returns documents accessed or modified within the past 7 days (maximum 50).

**Response:**
```json
{
  "success": true,
  "message": "Recent documents retrieved",
  "data": [
    { "name": "Sprint Report.docx", "modifiedBy": "Afzal Ahmed", "modifiedAt": "2024-01-14T16:00:00.000Z", "location": "Team/Reports" },
    { "name": "Architecture Decision Record.md", "modifiedBy": "Tom Harris", "modifiedAt": "2024-01-13T11:00:00.000Z", "location": "Engineering/ADRs" },
    { "name": "Q1 Planning.xlsx", "modifiedBy": "Sarah Khan", "modifiedAt": "2024-01-12T09:30:00.000Z", "location": "Finance/Planning" },
    { "name": "Client Proposal.pptx", "modifiedBy": "Emma Roberts", "modifiedAt": "2024-01-11T14:00:00.000Z", "location": "Sales/Proposals" }
  ],
  "errors": [],
  "timestamp": "2024-01-15T14:06:00.000Z",
  "correlationId": "b4c5d6e7-f8a9-0123-bcde-567890123456"
}
```

---

## Error Response Examples

### Validation Error (400)

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    { "field": "Name", "detail": "Name must be between 1 and 100 characters" },
    { "field": "Email", "detail": "A valid email address is required" }
  ],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "c5d6e7f8-a9b0-1234-cdef-678901234567"
}
```

### Not Found Error (404)

```json
{
  "success": false,
  "message": "Employee with ID 'f47ac10b-58cc-4372-a567-0e02b2c3d479' was not found",
  "data": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "d6e7f8a9-b0c1-2345-defa-789012345678"
}
```

### Server Error (500)

```json
{
  "success": false,
  "message": "An unexpected error occurred. Please reference the correlationId for support.",
  "data": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00.000Z",
  "correlationId": "e7f8a9b0-c1d2-3456-efab-890123456789"
}
```
