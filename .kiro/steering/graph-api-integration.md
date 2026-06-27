---
inclusion: auto
---

# GraphBridge Enterprise Suite — Microsoft Graph API Integration Standards

## Abstraction Layer

### Required Interfaces (Application Layer)
```csharp
IGraphUserService       // User creation, profile management, group assignment
IGraphGroupService      // Group CRUD, membership management
IGraphMailService       // Email sending, drafts, reading
IGraphCalendarService   // Events, meetings, scheduling
IGraphTeamsService      // Teams, channels, messaging concepts
IGraphDriveService      // OneDrive, SharePoint files
IGraphPlannerService    // Tasks, plans, buckets
IGraphSecurityService   // Security signals, alerts
IGraphReportService     // Usage reports, activity
```

### Implementation Pattern
Each interface has TWO implementations:
1. **Real:** Uses `Microsoft.Graph.GraphServiceClient` with proper auth
2. **Mock:** Returns realistic demo data without any Graph calls

### DI Registration
```csharp
if (configuration["GraphBridge:GraphMode"] == "Live")
{
    services.AddScoped<IGraphUserService, GraphUserService>();
    services.AddScoped<IGraphMailService, GraphMailService>();
    // ... all real implementations
}
else
{
    services.AddScoped<IGraphUserService, MockGraphUserService>();
    services.AddScoped<IGraphMailService, MockGraphMailService>();
    // ... all mock implementations
}
```

## Authentication Flow (Live Mode)
1. Application registers in Microsoft Entra ID
2. Client credential flow or delegated flow depending on scenario
3. Token acquired via MSAL / Azure.Identity
4. Token passed to GraphServiceClient
5. GraphServiceClient makes Graph API calls

## Permission Model Documentation
Each module documents which Graph permissions it requires:

### Module 1: Employee Onboarding
- User.Read.All (read user profiles)
- User.ReadWrite.All (create users)
- Group.ReadWrite.All (assign to groups)
- Mail.Send (send welcome email)
- Calendars.ReadWrite (schedule induction)

### Module 2: Legal Matter Workspace
- Sites.ReadWrite.All (SharePoint folders)
- Team.ReadBasic.All (Teams concepts)
- Channel.ReadBasic.All (channel creation)
- Calendars.ReadWrite (schedule kickoff)

### Module 3: Loan Approval
- Mail.Send (customer notification)
- Files.ReadWrite.All (approval folder)
- Calendars.ReadWrite (follow-up)

### Module 4: BuildEstate Project
- Tasks.ReadWrite (Planner tasks)
- Sites.ReadWrite.All (SharePoint)
- Mail.Send (director notifications)
- Calendars.ReadWrite (project kickoff)

### Module 5: CEO Command Centre
- Calendars.Read (today's meetings)
- Mail.Read (email summary)
- Tasks.ReadWrite (task status)
- Files.Read.All (document approvals)
- AuditLog.Read.All (security signals)

### Module 6: Productivity Assistant
- Calendars.Read (weekly events)
- Mail.Read (email summary)
- Tasks.ReadWrite (task summary)
- Files.Read.All (document activity)

## Error Handling for Graph Calls
- Wrap all Graph SDK calls in try-catch
- Log failures with correlation IDs
- Return meaningful error messages (not raw Graph exceptions)
- Handle common errors: 401 (auth expired), 403 (insufficient permissions), 429 (throttled), 404 (not found)
- Implement retry with exponential backoff for 429 responses
