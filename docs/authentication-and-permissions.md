# Authentication and Permissions

## Overview

GraphBridge Enterprise Suite authenticates with Microsoft 365 via **Microsoft Entra ID** (formerly Azure Active Directory) using the **Microsoft Authentication Library (MSAL)**. In Live_Mode, the application acquires OAuth 2.0 access tokens to call the Microsoft Graph API. In Demo_Mode, no authentication is required — the platform runs entirely with mock data.

---

## Entra ID App Registration

### Step 1: Create App Registration

1. Navigate to the [Azure Portal](https://portal.azure.com) → Microsoft Entra ID → App registrations
2. Click **New registration**
3. Configure:
   - **Name:** GraphBridge Enterprise Suite
   - **Supported account types:** Accounts in this organizational directory only (Single tenant)
   - **Redirect URI:** `https://localhost:7001/signin-oidc` (for development)
4. Click **Register**
5. Note the **Application (client) ID** and **Directory (tenant) ID**

### Step 2: Configure Client Secret

1. In the app registration, navigate to **Certificates & secrets**
2. Click **New client secret**
3. Set description: "GraphBridge Development" and expiry period
4. Click **Add**
5. Copy the secret value immediately (it will not be shown again)

### Step 3: Configure API Permissions

1. Navigate to **API permissions**
2. Click **Add a permission** → **Microsoft Graph**
3. Add the permissions listed in the table below
4. Click **Grant admin consent** for the tenant

### Step 4: Configure Application Settings

Update `appsettings.Development.json` (not committed to source control):

```json
{
  "GraphBridge": {
    "GraphMode": "Live",
    "AzureAd": {
      "TenantId": "<your-tenant-id>",
      "ClientId": "<your-client-id>",
      "ClientSecret": "<your-client-secret>",
      "RedirectUri": "https://localhost:7001/signin-oidc"
    }
  }
}
```

---

## Permissions Table

### Required Microsoft Graph Permissions

| Permission | Type | Description | Used By |
|-----------|------|-------------|---------|
| `User.Read.All` | Application | Read all users' full profiles | Onboarding, CEO Dashboard |
| `User.ReadWrite.All` | Application | Create and manage user accounts | Onboarding |
| `Group.Read.All` | Application | Read all groups | Onboarding |
| `Group.ReadWrite.All` | Application | Create groups, manage membership | Onboarding |
| `Mail.Send` | Application | Send mail as any user | Onboarding, Loan Approval, BuildEstate |
| `Mail.Read` | Application | Read mail in all mailboxes | CEO Dashboard, Productivity |
| `Calendars.ReadWrite` | Application | Create and manage calendar events | Onboarding, Legal Matter, Loan Approval, BuildEstate, CEO Dashboard, Productivity |
| `Sites.ReadWrite.All` | Application | Create and manage SharePoint sites/folders | Legal Matter, BuildEstate |
| `Files.ReadWrite.All` | Application | Create and manage files in SharePoint | Legal Matter, BuildEstate, CEO Dashboard, Productivity |
| `Channel.Create` | Application | Create Teams channels | Legal Matter |
| `ChannelMessage.Send` | Application | Send messages to Teams channels | Loan Approval |
| `Team.ReadBasic.All` | Application | Read Teams information | Legal Matter, Loan Approval |
| `Tasks.ReadWrite.All` | Application | Create and manage Planner tasks | BuildEstate, CEO Dashboard, Productivity |
| `SecurityEvents.Read.All` | Application | Read security alerts | CEO Dashboard |
| `Reports.Read.All` | Application | Read usage reports | CEO Dashboard, Productivity |

---

## Delegated vs Application Permissions

### Delegated Permissions

- Act on behalf of the signed-in user
- Require user to be signed in
- Access limited to what the user can access
- Token includes user context (user ID, name, roles)
- Typically used for user-facing operations

**Example:** A user reading their own calendar events.

### Application Permissions

- Act as the application without a user present
- Require admin consent
- Access tenant-wide data
- Token is app-only (no user context)
- Used for background/daemon scenarios or admin operations

**Example:** Sending a welcome email on behalf of HR when onboarding a new employee.

### GraphBridge Permission Strategy

GraphBridge uses **Application Permissions** for all Graph operations because:

1. **Automation scenarios** — Onboarding, workspace creation, and notifications happen without the target user being signed in
2. **Cross-user access** — CEO Dashboard aggregates data across users and services
3. **Background operations** — Communication workflows run asynchronously
4. **Consistency** — A single permission model simplifies DI and token management

If a future requirement needs user-delegated access (e.g., user-specific consent flows), the architecture supports adding a delegated flow alongside the existing application flow.

---

## Admin Consent

### What Requires Admin Consent

All Application permissions require tenant administrator consent. This is a one-time operation per tenant.

### Granting Admin Consent

**Option A: Azure Portal**
1. Navigate to Microsoft Entra ID → App registrations → GraphBridge Enterprise Suite
2. Go to API permissions
3. Click **Grant admin consent for [Tenant Name]**
4. Confirm the consent prompt

**Option B: Admin Consent URL**
Navigate to:
```
https://login.microsoftonline.com/{tenant-id}/adminconsent
?client_id={client-id}
&redirect_uri=https://localhost:7001/signin-oidc
```

### Consent Requirements

| Scenario | Consent Required |
|----------|-----------------|
| First deployment to new tenant | Full admin consent for all permissions |
| Adding new permissions | Incremental admin consent for new scopes |
| Demo_Mode operation | No consent required (no Graph calls made) |

---

## Azure Key Vault Integration for Production

### Overview

In production environments, secrets (ClientSecret, connection strings, certificates) must **never** be stored in configuration files or environment variables on the host. Azure Key Vault provides secure, centralized secret management with audit logging and access policies.

### Architecture

```
┌─────────────────────┐         ┌──────────────────┐
│  GraphBridge API    │──────── │  Azure Key Vault  │
│  (App Service)      │  MSI    │                  │
│                     │────────▶│  - ClientSecret   │
│  Managed Identity   │         │  - TenantId       │
│                     │         │  - ClientId        │
└─────────────────────┘         └──────────────────┘
```

### Step 1: Create Key Vault

```bash
az keyvault create \
  --name graphbridge-kv \
  --resource-group graphbridge-rg \
  --location uksouth
```

### Step 2: Store Secrets

```bash
az keyvault secret set \
  --vault-name graphbridge-kv \
  --name "GraphBridge--AzureAd--TenantId" \
  --value "<your-tenant-id>"

az keyvault secret set \
  --vault-name graphbridge-kv \
  --name "GraphBridge--AzureAd--ClientId" \
  --value "<your-client-id>"

az keyvault secret set \
  --vault-name graphbridge-kv \
  --name "GraphBridge--AzureAd--ClientSecret" \
  --value "<your-client-secret>"
```

> **Note:** The `--` separator maps to the `:` configuration hierarchy in .NET (e.g., `GraphBridge--AzureAd--ClientId` becomes `GraphBridge:AzureAd:ClientId`).

### Step 3: Enable Managed Identity on App Service

```bash
az webapp identity assign \
  --resource-group graphbridge-rg \
  --name graphbridge-api
```

Note the `principalId` returned by this command.

### Step 4: Grant Key Vault Access

```bash
az keyvault set-policy \
  --name graphbridge-kv \
  --object-id <principal-id> \
  --secret-permissions get list
```

### Step 5: Configure Application to Use Key Vault

In `Program.cs`, add the Key Vault configuration provider:

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://graphbridge-kv.vault.azure.net/"),
    new DefaultAzureCredential());
```

The `DefaultAzureCredential` automatically uses Managed Identity in Azure and falls back to developer credentials locally.

### Step 6: Verify Secret Rotation Support

Key Vault secrets can be rotated without redeploying the application. Configure:
- Secret expiration notifications in Key Vault
- Automatic reload interval in the configuration provider (default: 24 hours)
- Application restart strategy for immediate secret refresh

### Production Checklist

| Item | Status |
|------|--------|
| Key Vault created in production resource group | ☐ |
| All secrets stored in Key Vault (not in app settings) | ☐ |
| Managed Identity enabled on App Service | ☐ |
| Key Vault access policy grants Get + List to MI | ☐ |
| Application uses `AddAzureKeyVault` in Program.cs | ☐ |
| Network restrictions configured (VNet/Private Endpoint) | ☐ |
| Diagnostic logging enabled on Key Vault | ☐ |
| Secret rotation policy configured | ☐ |
| `appsettings.Development.json` in .gitignore | ☐ |
| No secrets in source control history | ☐ |

---

## Token Acquisition and Caching

### MSAL Flow

GraphBridge uses the **Client Credentials** flow (OAuth 2.0 client_credentials grant):

```
GraphBridge API → MSAL → Microsoft Entra ID → Access Token → Graph API
```

### Token Caching Strategy

- Tokens are cached in memory using MSAL's built-in token cache
- Cached tokens are served until **5 minutes before expiry**
- When a token is within 5 minutes of expiry, MSAL acquires a fresh token
- If token acquisition fails (network error, invalid credentials), the system returns HTTP 401

### Error Handling

| Failure | Response |
|---------|----------|
| Token expired or invalid | 401 Unauthorized with API_Envelope |
| Network error during acquisition | 401 Unauthorized with API_Envelope |
| Invalid client credentials | 401 Unauthorized with API_Envelope |
| Tenant not found | 401 Unauthorized with API_Envelope |

All authentication failures include the `correlationId` in both the response body and structured log output for troubleshooting.
