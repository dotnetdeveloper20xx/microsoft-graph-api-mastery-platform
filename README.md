# GraphBridge Enterprise Suite

A portfolio-grade Microsoft Graph API Mastery Platform comprising six distinct business application modules. Each module demonstrates enterprise-level integration between custom business workflows and Microsoft 365 services via the Microsoft Graph API. The platform operates in dual mode — Live with real Graph SDK credentials against a Microsoft 365 tenant, or Demo with deterministic realistic mock data requiring zero external configuration.

---

## Architecture Overview

GraphBridge follows Clean Architecture with strict compile-time layer separation:

```
┌─────────────────────────────────────────────────────┐
│                   GraphBridge.Api                     │
│         ASP.NET Core Controllers + Middleware         │
├─────────────────────────────────────────────────────┤
│              GraphBridge.Infrastructure               │
│      Graph SDK Implementations + Mock Services        │
├─────────────────────────────────────────────────────┤
│              GraphBridge.Application                  │
│       CQRS Handlers + DTOs + Graph Interfaces         │
├─────────────────────────────────────────────────────┤
│                GraphBridge.Domain                     │
│          Entities + Enums + Value Objects              │
├─────────────────────────────────────────────────────┤
│                GraphBridge.Shared                     │
│     API Envelope + Exceptions + Constants             │
└─────────────────────────────────────────────────────┘
```

**Key Patterns:**
- CQRS via MediatR — every operation dispatched through command/query handlers
- FluentValidation pipeline — automatic request validation before handler execution
- Dual-mode DI — strategy pattern switches between mock and live Graph services
- Standardised API Envelope — consistent response format across all endpoints

---

## Technology Stack

### Backend
| Technology | Purpose |
|-----------|---------|
| .NET 8 | Runtime and SDK |
| ASP.NET Core | Web API framework |
| MediatR | CQRS command/query dispatch |
| FluentValidation | Request validation |
| Microsoft Graph SDK | Microsoft 365 API integration |
| MSAL (Microsoft.Identity) | OAuth 2.0 token acquisition |
| Serilog | Structured logging |
| xUnit | Unit and integration testing |
| FsCheck | Property-based testing |
| FluentAssertions | Test assertion library |
| Moq | Mocking framework |

### Frontend
| Technology | Purpose |
|-----------|---------|
| Angular 20 | SPA framework |
| Standalone Components | Modern Angular architecture |
| Signals | Reactive state management |
| Lazy-loaded Routes | Performance-optimised navigation |
| TypeScript | Type-safe development |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (for Angular frontend)
- [Angular CLI 20](https://angular.dev/) (`npm install -g @angular/cli`)

**For Live_Mode only:**
- Microsoft 365 tenant with admin access
- Azure subscription (for Entra ID app registration)
- See [Authentication and Permissions](./docs/authentication-and-permissions.md) for full setup

---

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/microsoft-graph-api-mastery-platform.git
cd microsoft-graph-api-mastery-platform
```

### 2. Run the Backend (Demo_Mode — no credentials required)

```bash
dotnet restore
dotnet build
dotnet run --project src/GraphBridge.Api
```

The API will start at `https://localhost:7001` in Demo_Mode by default.

### 3. Run the Frontend

```bash
cd client
npm install
ng serve
```

The Angular app will start at `http://localhost:4200` and proxy API calls to the backend.

### 4. Run Tests

```bash
# Unit tests
dotnet test tests/GraphBridge.UnitTests

# Integration tests
dotnet test tests/GraphBridge.IntegrationTests

# All tests
dotnet test
```

### 5. Switch to Live_Mode (Optional)

1. Complete Entra ID app registration (see [docs](./docs/authentication-and-permissions.md))
2. Create `src/GraphBridge.Api/appsettings.Development.json`:

```json
{
  "GraphBridge": {
    "GraphMode": "Live",
    "AzureAd": {
      "TenantId": "<your-tenant-id>",
      "ClientId": "<your-client-id>",
      "ClientSecret": "<your-client-secret>"
    }
  }
}
```

3. Restart the backend

---

## Demo_Mode

Demo_Mode is the default operating mode. It allows the entire platform to run without Microsoft Graph credentials, making it ideal for:

- **Portfolio demonstration** — show the platform to reviewers without tenant access
- **Local development** — work on business logic without Graph API dependencies
- **CI/CD testing** — run integration tests without external services
- **Learning** — explore Graph API patterns without Azure configuration

In Demo_Mode, all Graph service interfaces return deterministic, realistic business data:
- Named employees (Sarah Khan, Afzal Ahmed)
- Real company names (Oakfield Estates Ltd, Greenway Property Holdings)
- Meaningful document structures and calendar events
- Realistic security alerts and activity metrics

The mode is configured via `GraphBridge:GraphMode` in `appsettings.json`. If the setting is absent or invalid, the platform defaults to Demo_Mode with a warning log.

---

## Modules

| # | Module | Description |
|---|--------|-------------|
| 1 | **Employee Onboarding** | Automates Microsoft 365 provisioning for new hires: groups, welcome email, induction scheduling |
| 2 | **Legal Matter Workspace** | Provisions SharePoint folders, Teams channels, and kickoff meetings for legal cases |
| 3 | **Loan Approval Communication Hub** | Orchestrates customer emails, team notifications, and follow-up scheduling with audit trail |
| 4 | **BuildEstate Project Launch** | Creates project workspaces with task boards, director notifications, and kickoff scheduling |
| 5 | **CEO Command Centre** | Aggregates calendar, email, tasks, documents, and security signals into an executive dashboard |
| 6 | **AI Productivity Assistant** | Generates weekly productivity summaries and AI-ready context packages from Microsoft 365 data |

---

## Project Structure

```
microsoft-graph-api-mastery-platform/
├── src/
│   ├── GraphBridge.Api/              # Controllers, middleware, Program.cs
│   ├── GraphBridge.Application/      # CQRS handlers, DTOs, validators, interfaces
│   ├── GraphBridge.Domain/           # Entities, enums, value objects
│   ├── GraphBridge.Infrastructure/   # Graph SDK + mock implementations
│   └── GraphBridge.Shared/           # API envelope, exceptions, constants
├── tests/
│   ├── GraphBridge.UnitTests/        # Unit + property-based tests
│   └── GraphBridge.IntegrationTests/ # TestServer integration tests
├── client/                           # Angular 20 frontend
├── docs/                             # Detailed documentation
│   ├── architecture.md
│   ├── microsoft-graph-overview.md
│   ├── authentication-and-permissions.md
│   ├── module-guide.md
│   └── endpoint-catalog.md
├── GraphBridge.sln
└── README.md
```

---

## Documentation

For detailed documentation, see the [/docs](./docs/) folder:

- [Architecture](./docs/architecture.md) — Clean Architecture layers, CQRS pattern, DI strategy
- [Microsoft Graph Overview](./docs/microsoft-graph-overview.md) — Graph API capabilities demonstrated per module
- [Authentication & Permissions](./docs/authentication-and-permissions.md) — Entra ID setup, permissions, Key Vault
- [Module Guide](./docs/module-guide.md) — Business scenarios, endpoints, and routes per module
- [Endpoint Catalog](./docs/endpoint-catalog.md) — Complete API reference with request/response examples

---

## License

See [LICENSE](./LICENSE) for details.
