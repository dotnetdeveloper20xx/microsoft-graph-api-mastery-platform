---
inclusion: auto
---

# GraphBridge Enterprise Suite — Security Standards

## Authentication
- Microsoft Entra ID (Azure AD) for identity
- JWT Bearer tokens for API authentication
- Token handling via MSAL / Azure.Identity
- Backend prepared for JWT bearer authentication

## Graph API Security
- Use Microsoft Identity Platform properly
- Support both delegated and application permissions
- Document required Graph permissions per module
- Use least privilege principle for Graph scopes
- Never store tokens in source code
- Use appsettings.Development.json placeholders only
- Key Vault for production secrets

### Required Graph Permissions (Documented)
- User.Read, User.Read.All
- Group.Read.All, Group.ReadWrite.All
- Mail.Read, Mail.Send
- Calendars.Read, Calendars.ReadWrite
- Files.Read.All, Files.ReadWrite.All
- Sites.Read.All, Sites.ReadWrite.All
- Team.ReadBasic.All, Channel.ReadBasic.All
- Tasks.ReadWrite
- Directory.Read.All
- AuditLog.Read.All

## App Registration Configuration
Document placeholders for:
- Tenant ID
- Client ID
- Client Secret or certificate
- Redirect URI
- Scopes
- App permissions vs Delegated permissions
- Admin consent

## Input Validation
- Validate ALL input at API boundary (FluentValidation)
- Validate type, length, format, range
- Parameterized queries only
- Sanitize output to prevent XSS

## API Security
- HTTPS only
- CORS restricted to known origins
- No sensitive data in URLs
- No sensitive data in logs
- Global exception handler hides internals

## Secure Coding Practices
- No hardcoded secrets
- No sensitive data in error messages
- No stack traces in production responses
- Connection strings from environment / Key Vault
- Dispose sensitive data after use
