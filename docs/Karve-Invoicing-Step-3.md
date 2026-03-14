
# 🔐 **STEP 3 — Authentication + Multi‑Tenant Enforcement + Global Query Filters**

## 🎯 Step 3 Goals
By the end of Step 3, your API will:

- Authenticate users via Azure AD (PKCE for SPA later)  
- Automatically provision local `AppUser` records  
- Enforce company membership  
- Apply global query filters for multi‑tenancy  
- Restrict all data access to the user’s companies  
- Provide a clean `ICurrentUserService` abstraction  
- Reject unauthorized or unassigned users  
- Pass tests for authentication + tenancy  

---

# 🧩 Task Group A — Configure Azure AD Authentication

### **A1 — Add Microsoft Identity Web packages**
In the API project:

```bash
dotnet add src/Karve.Invoicing.Api package Microsoft.Identity.Web
dotnet add src/Karve.Invoicing.Api package Microsoft.Identity.Web.MicrosoftGraph
```

### **A2 — Add authentication configuration to `appsettings.json`**
Add:

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "<your-tenant-id>",
  "ClientId": "<your-api-client-id>",
  "Audience": "<your-api-client-id>"
}
```

What goes in each field:
- `Instance`: always `https://login.microsoftonline.com/` for Azure AD.
- `TenantId`: your Azure AD **Directory (tenant) ID** — found on the Azure AD overview page or the app registration overview.
- `ClientId`: the **Application (client) ID** of your **API app registration** — this is the app that the back-end API itself is registered as in Azure AD.
- `Audience`: should match `ClientId` for a single-tenant API, or use the full App ID URI (e.g., `api://<client-id>`) if that's how you exposed your API. Microsoft.Identity.Web validates the incoming token audience against this value.

### **A3 — Configure authentication in `Program.cs`**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
```

### **A4 — Add authorization middleware**
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### **A5 — Add `[Authorize]` to all controllers**
At the class level.

---

# 🧩 Task Group B — Implement `ICurrentUserService`

### **B1 — Create `/Services` folder in Application project**

### **B2 — Create `ICurrentUserService` interface**
Methods:
- `string? UserId { get; }`
- `string? Email { get; }`
- `IReadOnlyList<Guid> CompanyIds { get; }`

### **B3 — Implement `CurrentUserService` in API project**
Extract from JWT:
- `oid` → UserId  
- `preferred_username` or `email` → Email  

Company IDs will be loaded later.

### **B4 — Register service in DI**
```csharp
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

---

# 🧩 Task Group C — Automatic User Provisioning

### **C1 — Create `IUserProvisioningService` in Application project**

### **C2 — Implement `UserProvisioningService` in Infrastructure**
Responsibilities:
- Check if `AppUser` exists by external ID  
- If not, create a new `AppUser`  
- Do **not** assign a company yet  
- Save to database  

### **C3 — Create middleware: `UserProvisioningMiddleware`**
Steps:
1. If request is authenticated  
2. Extract external user ID  
3. Call `UserProvisioningService`  
4. Attach local user ID to `HttpContext.Items`  

### **C4 — Register middleware**
```csharp
app.UseMiddleware<UserProvisioningMiddleware>();
```

---

# 🧩 Task Group D — Company Membership Enforcement

### **D1 — Create `ICompanyMembershipService`**
Methods:
- `Task<IReadOnlyList<Guid>> GetCompanyIdsForUserAsync(Guid userId)`
- `Task<bool> UserBelongsToCompanyAsync(Guid userId, Guid companyId)`

### **D2 — Implement `CompanyMembershipService` in Infrastructure**

### **D3 — Update `CurrentUserService`**
Load company IDs via `ICompanyMembershipService`.

### **D4 — Create authorization policy: `RequireCompanyMembership`**
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireCompanyMembership", policy =>
        policy.RequireAssertion(context =>
        {
            var svc = context.User.GetRequiredService<ICurrentUserService>();
            return svc.CompanyIds.Any();
        }));
});
```

### **D5 — Apply policy globally**
```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter("RequireCompanyMembership"));
});
```

---

# 🧩 Task Group E — Global Query Filters for Multi‑Tenancy

### **E1 — Update `InvoicingDbContext` constructor**
Inject `ICurrentUserService`.

### **E2 — Add global filters in `OnModelCreating`**
For each entity with `CompanyId`:

```csharp
modelBuilder.Entity<Customer>()
    .HasQueryFilter(e => currentUser.CompanyIds.Contains(e.CompanyId));
```

Repeat for:
- Company  
- Customer  
- Product  
- Invoice  
- Payment  

### **E3 — Add guard for unauthenticated users**
If no user or no companies → return no data.

---

# 🧩 Task Group F — Update Repositories for Tenant Enforcement

### **F1 — Ensure all repository queries respect global filters**
No changes needed if filters are correct.

### **F2 — Add guard clauses for cross‑company access**
Example in `InvoiceRepository`:

```csharp
if (!currentUser.CompanyIds.Contains(invoice.CompanyId))
    throw new ForbiddenException("User does not belong to this company.");
```

### **F3 — Add `ForbiddenException` class**
In Application project.

---

# 🧩 Task Group G — Update Controllers for Tenant Enforcement

### **G1 — Remove any CompanyId parameters from requests**
CompanyId is derived from the user.

### **G2 — When creating entities, set `CompanyId` automatically**
Example:

```csharp
entity.CompanyId = currentUser.CompanyIds.Single();
```

(If multiple companies per user, prompt selection later.)

### **G3 — Ensure all GET/PUT/DELETE operations rely on global filters**
No manual filtering needed.

---

# 🧩 Task Group H — Add Tenant Resolution Middleware

### **H1 — Create `TenantResolutionMiddleware`**
Responsibilities:
- Ensure user has at least one company  
- If not → return 403  
- If user has multiple companies → store list in HttpContext for future selection  

### **H2 — Register middleware**
Place after authentication, before controllers.

---

# 🧩 Task Group I — Add Tests for Authentication + Tenancy

### **I1 — Add test helpers for authenticated requests**
Use `WebApplicationFactory` + fake JWT.

### **I2 — Add tests for:**
- User provisioning  
- Company membership enforcement  
- Global query filters  
- Forbidden access  
- Allowed access  
- Controllers returning only tenant‑scoped data  

### **I3 — Add tests for `ICurrentUserService`**

---

# 🧩 Task Group J — Update OpenAPI + Scalar

### **J1 — Add security scheme to OpenAPI**
```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddSecurityScheme("oauth2", ...);
});
```

### **J2 — Add “Authorize” button in Scalar**
Configure OAuth settings.

### **J3 - Local PKCE setup values (Azure AD + Scalar)**
Use these exact mappings in `src/Karve.Invoicing.Api/appsettings.json`:

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "<directory-tenant-id>",
  "ClientId": "<api-app-registration-client-id>",
  "Audience": "<api-app-registration-client-id>"
},
"OpenApi": {
  "OAuthClientId": "<SPA-APP-REGISTRATION-CLIENT-ID>",
  "OAuthScope": "api://<API-APP-ID-URI-OR-API-CLIENT-ID>/user_impersonation"
}
```

How to choose the values:
- `AzureAd:TenantId`: your Azure AD **Directory (tenant) ID** — found on the Azure AD overview page.
- `AzureAd:ClientId`: the **Application (client) ID** of the **API app registration** (the back-end). This is used to validate tokens sent to the API.
- `AzureAd:Audience`: same as `ClientId` for single-tenant APIs. Use the full App ID URI (e.g., `api://<api-client-id>`) if you configured a custom App ID URI and want to validate against it.
- `OpenApi:OAuthClientId`: the **client ID of your SPA app registration** (the app users sign in to from the browser/Scalar UI — can be different to the API app registration).
    - `Expose an API` configured with App ID URI (for example `api://<api-client-id>`).
    - Delegated scope `user_impersonation` exists.
- SPA app registration:
    - Has a SPA redirect URI used by Scalar OAuth flow.
    - Has delegated API permission for `user_impersonation`.
    - Admin/user consent granted for that delegated permission.
- API authorization:
    - API validates tokens for the same tenant and audience configured in `AzureAd`.

Notes:
- If your API App ID URI is customized (not `api://<api-client-id>`), use that exact URI prefix in `OpenApi:OAuthScope`.
- If you set `flow.RedirectUri` in `Program.cs`, the same URI must be added as a SPA redirect URI in Azure AD.

---

# 🎉 Step 3 Complete  
Once you finish these tasks, your API will be:

- Fully authenticated  
- Multi‑tenant aware  
- Secure by default  
- Enforcing company boundaries  
- Returning only tenant‑scoped data  
- Ready for the React OAuth PKCE flow in Step 4  

This is a major milestone — your backend is now “real product” quality.

