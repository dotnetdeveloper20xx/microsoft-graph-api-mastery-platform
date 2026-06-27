---
inclusion: auto
---

# GraphBridge Enterprise Suite — Coding Standards

## Naming Conventions

### C# / .NET
| Element | Convention | Example |
|---------|-----------|---------|
| Classes/Interfaces | PascalCase | `EmployeeOnboarding`, `IGraphUserService` |
| Methods | PascalCase | `GetOnboardingStatusAsync` |
| Properties | PascalCase | `EmployeeName` |
| Private fields | _camelCase | `_graphUserService` |
| Parameters/locals | camelCase | `employeeId` |
| Constants | PascalCase | `MaxRetryCount` |
| Interfaces | I prefix | `IGraphMailService` |
| Async methods | Async suffix | `SendWelcomeEmailAsync` |

### TypeScript / Angular
| Element | Convention | Example |
|---------|-----------|---------|
| Components | PascalCase + suffix | `OnboardingListComponent` |
| Services | PascalCase + suffix | `OnboardingService` |
| Interfaces | I prefix | `IOnboardingStatus` |
| Files | kebab-case | `onboarding-list.component.ts` |
| Variables/functions | camelCase | `getOnboardingOverview` |
| Constants | UPPER_SNAKE_CASE | `MAX_PAGE_SIZE` |

## Git Conventions
- Branch: `feature/{module}/{short-description}`
- Commits: Conventional Commits (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`)
- One feature per branch
- PR required for merge to main

## File Organisation
- One class per file
- One component per file
- Feature-based folder structure (not type-based)
- Keep related files close together
- No utility dumping grounds

## Code Quality Rules
- Clear naming over comments
- Small focused classes
- No god services
- No business logic in controllers
- No direct Graph SDK usage in controllers
- No direct Graph SDK models returned to Angular
- Use async/await properly
- Use cancellation tokens
- Use nullable reference types
- Use dependency injection
- Use FluentValidation
- Use typed DTOs
- Use consistent API response objects
- Use environment-based configuration
- Use secure secret handling
