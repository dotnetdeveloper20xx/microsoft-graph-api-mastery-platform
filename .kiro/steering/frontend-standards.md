---
inclusion: auto
---

# GraphBridge Enterprise Suite — Frontend Standards

## Technology
- Angular 20 (Standalone Components)
- TypeScript (strict mode — no `any`)
- Angular Signals for local component state
- NgRx or signal-based store for feature-level state
- RxJS for API calls
- SCSS or Tailwind CSS
- Route-based lazy loading

## Component Architecture

### Smart (Container) Components
- Connect to store/state
- Dispatch actions or call services
- Handle routing logic
- Pass data down to presentational components

### Presentational (Dumb) Components
- Receive data via @Input()
- Emit events via @Output()
- No store dependency
- Reusable and testable in isolation

### Component Rules
- No business logic in templates
- Use OnPush change detection strategy
- One component per file
- Keep component focused on a single UI concern
- Unsubscribe from observables properly

## State Management
- Use Signals for simple local state
- Use NgRx or signal store for feature-level shared state
- API calls belong in effects or services
- Components do NOT call APIs directly (go through services/store)

## Services
- One service per API resource / module
- Return Observable<T> (typed)
- Use HttpClient with interceptors
- Base URL from environment config
- Typed API clients matching backend DTOs

## Interceptors
- Auth token attachment (placeholder for Entra ID)
- Error handling (global)
- Loading indicator
- Correlation ID propagation

## Routing
- Lazy-loaded feature routes per module
- Route guards where appropriate
- Breadcrumb support via route data
- URL patterns: `/module-name`, `/module-name/:id`

## UI/UX Standards (Enterprise Dashboard)
Design to look like a serious Microsoft/Azure enterprise dashboard.

### Style Direction
- Clean, modern, professional
- Azure-inspired, calm
- Light theme
- Strong spacing and typography
- Dashboard cards, status chips, timelines
- Icons for visual hierarchy

### Every Module Landing Page Must Include
- What this module does
- Why Microsoft Graph matters here
- Business workflow diagram or visual stepper
- Key Graph API areas used
- Demo actions
- Results panel showing what happened

### Action Button Feedback
Each action button should show:
- What action was triggered
- Which backend endpoint was called
- Which Graph API capability this represents
- Demo result

## Master Dashboard (`/dashboard`)
Six large module cards showing:
- Business purpose
- Graph APIs used
- Number of demo workflows
- Status
- Open button

Plus a global "How Microsoft Graph Works" panel:
```
User/Application → Microsoft Entra ID → Access Token → Microsoft Graph API → Microsoft 365 Data/Actions → Business Workflow Result
```

## Error Handling
- HTTP interceptor catches all API errors
- User-friendly error messages (never raw server errors)
- Toast notifications for transient errors
- Loading states on all async operations
- Empty states when no data exists

## TypeScript Standards
- Strict mode enabled
- No `any` type
- Use interfaces for contracts
- Use enums for finite sets
- Prefer `readonly` where immutability is intended

## File Naming
- Files: kebab-case (`employee-onboarding.component.ts`)
- Components: PascalCase suffix (`EmployeeOnboardingComponent`)
- Services: PascalCase suffix (`OnboardingService`)
- Interfaces: prefix with `I` (`IOnboardingStatus`)
