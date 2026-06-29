# 🚀 GraphBridge Enterprise Suite

**A portfolio-grade Microsoft Graph API Mastery Platform demonstrating deep expertise in enterprise .NET development, Microsoft 365 integration, and modern Angular architecture.**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Angular 20](https://img.shields.io/badge/Angular-20-red)](https://angular.dev/)
[![Microsoft Graph](https://img.shields.io/badge/Microsoft%20Graph-SDK-0078D4)](https://learn.microsoft.com/en-us/graph/)
[![Tests](https://img.shields.io/badge/Tests-241%20Passing-brightgreen)]()
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)]()

---

## 💡 Why I Built This

I built GraphBridge to prove something: that I can design, architect, and implement a **production-grade platform** that integrates deeply with the Microsoft 365 ecosystem — not just call an API endpoint, but orchestrate complex multi-service workflows across Users, Groups, Mail, Calendar, Teams, SharePoint, Planner, and Security.

This isn't a tutorial or a toy. It's six fully-realised business modules, each representing real-world enterprise automation scenarios I've encountered in my career. Every module demonstrates how Microsoft Graph API can transform manual business processes into automated, auditable workflows.

**What this project proves:**
- I can architect a Clean Architecture solution with strict layer separation enforced at compile time
- I understand CQRS, MediatR pipelines, and how to build extensible command/query systems
- I can integrate with 9 Microsoft Graph API areas (Users, Groups, Mail, Calendar, Teams, Drive, Planner, Security, Reports) through proper abstraction layers
- I build testable systems — 241 tests including property-based tests that verify correctness invariants, not just examples
- I think about production concerns: dual-mode operation, token caching, correlation IDs, structured logging, and graceful degradation

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        GraphBridge.Api                                    │
│              ASP.NET Core 8 · Controllers · Middleware                    │
│         CorrelationId Tracing · Global Exception Handling                │
├─────────────────────────────────────────────────────────────────────────┤
│                     GraphBridge.Infrastructure                            │
│         9 Live Graph SDK Services · 9 Mock Services · MSAL Auth          │
│              In-Memory Stores · DI Registration                          │
├─────────────────────────────────────────────────────────────────────────┤
│                     GraphBridge.Application                               │
│       30+ CQRS Handlers · FluentValidation · MediatR Pipeline            │
│           9 Graph Service Interfaces · DTOs · Behaviors                   │
├─────────────────────────────────────────────────────────────────────────┤
│                       GraphBridge.Domain                                  │
│            Entities · Enums · Value Objects · Zero Dependencies           │
└─────────────────────────────────────────────────────────────────────────┘
```

**Key Design Decisions:**
| Decision | Choice | Why |
|----------|--------|-----|
| Architecture | Clean Architecture (5 projects) | Compile-time dependency enforcement; Domain stays pure |
| CQRS | MediatR with pipeline behaviors | Separation of reads/writes; cross-cutting validation/logging |
| Graph Abstraction | 9 interfaces in Application layer | Zero SDK leakage into controllers; fully testable |
| Dual Mode | Strategy pattern via DI | Demo without credentials; Live with real Graph SDK |
| Testing | xUnit + FsCheck (property-based) | Correctness invariants, not just happy-path examples |
| Frontend | Angular 20 standalone + signals | Modern patterns; lazy-loaded; signal-based state |

---

## 📊 Microsoft Graph API Coverage

This platform demonstrates mastery across **9 Graph API areas** — not just basic CRUD, but orchestrated multi-service workflows:

| Graph API Area | Interface | What I'm Demonstrating |
|----------------|-----------|----------------------|
| **Users** | `IGraphUserService` | Profile provisioning, user management |
| **Groups** | `IGraphGroupService` | Department-based auto-assignment, membership |
| **Mail** | `IGraphMailService` | Transactional emails, volume analytics, priority grouping |
| **Calendar** | `IGraphCalendarService` | Multi-attendee scheduling, date-range queries |
| **Teams** | `IGraphTeamsService` | Channel creation, channel notifications |
| **SharePoint/Drive** | `IGraphDriveService` | Folder structure provisioning, document tracking |
| **Planner** | `IGraphPlannerService` | Task board creation, status aggregation |
| **Security** | `IGraphSecurityService` | Alert monitoring, anomaly detection |
| **Reports** | `IGraphReportService` | Activity analytics, usage summaries |

### How Graph API Flows Through the Architecture

```
Angular UI → HTTP Request → Controller → mediator.Send()
    → ValidationBehavior (FluentValidation)
    → LoggingBehavior (CorrelationId + timing)
    → CQRS Handler
    → IGraph*Service (injected interface)
    → Mock Implementation (Demo) OR Live Graph SDK (Production)
    → Response DTO → ApiEnvelope → HTTP Response
```

The Graph SDK never touches a controller. It never touches a handler. It lives exclusively in the Infrastructure layer behind interfaces, making every single business operation fully testable without network calls.

---

## 🏢 The Six Business Modules

Each module represents a real enterprise scenario where Microsoft Graph API transforms manual processes:

### Module 1: Employee Onboarding Automation
*"New hire starts Monday. Manually creating accounts, assigning groups, sending welcome emails, scheduling inductions? That's 45 minutes of HR time per employee."*

**Graph APIs:** Users → Groups → Mail → Calendar (4-step automated workflow)

### Module 2: Legal Matter Workspace Automation
*"Solicitor opens a new case. They need SharePoint folders, a Teams channel, participant access, and a kickoff meeting — all named consistently."*

**Graph APIs:** Drive → Teams → Calendar (workspace provisioning with idempotency guards)

### Module 3: Loan Approval Communication Hub
*"Loan approved. Now send the customer email, notify the finance team on Teams, schedule follow-up — and audit every step."*

**Graph APIs:** Mail → Teams → Calendar (ordered workflow with audit trail enforcement)

### Module 4: BuildEstate Project Launch
*"Planning approval granted. Spin up the full project workspace: SharePoint docs, Planner task board, director notifications, kickoff meeting."*

**Graph APIs:** Drive → Planner → Mail → Calendar (multi-service workspace orchestration)

### Module 5: CEO Command Centre Dashboard
*"Show me everything: today's meetings, unread emails by priority, pending tasks, document approvals, security alerts — in one view."*

**Graph APIs:** Calendar + Mail + Planner + Drive + Security (5-service aggregation with partial failure handling)

### Module 6: AI Productivity Assistant
*"Generate my weekly productivity package: calendar events, email volume, task completion rates, recent documents — structured for AI consumption."*

**Graph APIs:** Calendar + Mail + Planner + Drive + Reports (7-day aggregation with AI-ready output)

---

## 🧪 Testing Philosophy

I don't just write tests that check happy paths. I write **property-based tests** that verify correctness invariants hold across all possible inputs:

```
241 Tests Total
├── Unit Tests (handlers, validators, mock services, middleware)
├── Property-Based Tests (FsCheck — 24 formal correctness properties)
└── Integration Tests (WebApplicationFactory end-to-end workflows)
```

**Example correctness properties I verify:**
- "For ANY exception type thrown, the response ALWAYS contains a valid ApiEnvelope with no stack traces leaked"
- "For ANY sequence of N requests, ALL correlation IDs are unique valid GUIDs"
- "For ANY loan NOT in Approved status, generate-pack ALWAYS fails — the handler is NEVER invoked"
- "For ANY valid creation request, the returned ID is ALWAYS a unique GUID that differs from all previous IDs"
- "For ANY combination of Graph service failures, the CEO Dashboard ALWAYS returns partial results rather than failing entirely"

This is formal verification thinking applied to business software.

---

## ⚡ Quick Start (No Microsoft Credentials Required)

```bash
# Clone and run — Demo Mode works out of the box
git clone https://github.com/dotnetdeveloper20xx/microsoft-graph-api-mastery-platform.git
cd microsoft-graph-api-mastery-platform

# Backend
dotnet restore && dotnet build && dotnet run --project src/GraphBridge.Api

# Frontend (separate terminal)
cd client && npm install && ng serve

# Run all 241 tests
dotnet test
```

The platform starts in **Demo Mode** by default — realistic named business data, zero external dependencies.

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Runtime** | .NET 8, ASP.NET Core |
| **Architecture** | Clean Architecture, CQRS, MediatR |
| **Validation** | FluentValidation with MediatR pipeline |
| **Graph Integration** | Microsoft Graph SDK, MSAL, Azure.Identity |
| **Frontend** | Angular 20, Standalone Components, Signals, Lazy Loading |
| **Testing** | xUnit, FsCheck (property-based), FluentAssertions, Moq |
| **Logging** | Serilog (structured, correlation-aware) |
| **Auth** | Microsoft Entra ID, MSAL, Token Caching (5-min buffer) |
| **Serialization** | System.Text.Json |

---

## 📁 Project Structure

```
├── src/
│   ├── GraphBridge.Api/              → Controllers, middleware, startup
│   ├── GraphBridge.Application/      → 30+ CQRS handlers, 9 interfaces, validators
│   ├── GraphBridge.Domain/           → Pure entities (zero dependencies)
│   ├── GraphBridge.Infrastructure/   → 9 Live + 9 Mock Graph services, MSAL
│   └── GraphBridge.Shared/           → ApiEnvelope, typed exceptions
├── tests/
│   ├── GraphBridge.UnitTests/        → Unit + property-based tests
│   └── GraphBridge.IntegrationTests/ → End-to-end workflow tests
├── client/                           → Angular 20 SPA
└── docs/                             → Architecture, Auth, API catalog
```

---

## 📖 Documentation

| Document | What's Inside |
|----------|--------------|
| [Architecture](./docs/architecture.md) | Clean Architecture layers, CQRS flow, DI strategy |
| [Microsoft Graph Overview](./docs/microsoft-graph-overview.md) | How each module uses Graph API |
| [Authentication & Permissions](./docs/authentication-and-permissions.md) | Entra ID setup, 15 permissions, Key Vault |
| [Module Guide](./docs/module-guide.md) | Business scenarios, endpoints, Angular routes |
| [Endpoint Catalog](./docs/endpoint-catalog.md) | Every endpoint with request/response examples |

---

## 🔑 Skills Demonstrated

- **Microsoft Graph API** — 9 service areas, orchestrated workflows, MSAL authentication
- **Clean Architecture** — Strict compile-time layer separation, dependency inversion
- **CQRS + MediatR** — Command/query separation, pipeline behaviors, cross-cutting concerns
- **ASP.NET Core (.NET 8)** — Middleware, DI, configuration, structured logging
- **Angular 20** — Standalone components, signals, lazy-loaded routes, HTTP interceptors
- **Testing** — Property-based testing (FsCheck), integration testing, 241 automated tests
- **Enterprise Patterns** — Correlation tracing, graceful degradation, audit trails, idempotency
- **Security** — MSAL token caching, Key Vault integration, zero secrets in source control
- **API Design** — Standardised envelope format, consistent error handling, field-level validation

---

## 👤 About Me

I'm a .NET developer who builds enterprise-grade systems. This project exists because I wanted to demonstrate — concretely, with working code — that I understand how to architect, implement, and test complex Microsoft 365 integrations at a level that goes far beyond basic API calls. https://dotnetdeveloper.co.uk

If you're reviewing this code, you're looking at how I think about software: layered, testable, production-aware, and built to handle the messy reality of distributed service integration.

---

## License

MIT
