# **Company Management + User Assignment (Admin‑Only)**

## Goals
By the end of this task group, you will have:

- A secure admin‑only UI for managing companies  
- Full CRUD for companies  
- Ability to assign/unassign users to companies  
- Backend endpoints protected by Azure AD role “Global Administrator”  
- React UI visible only to Global Admins  
- React Query hooks + forms + tables  
- Observability + validation + resilience baked in  

---

# Backend Task Group A — Authorization for Global Administrators

### **A1 — Add “Global Administrator” role requirement**
In `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GlobalAdminOnly", policy =>
        policy.RequireRole("Global Administrator"));
});
```

### **A2 — Add `[Authorize(Policy = "GlobalAdminOnly")]` to Company admin controller**
This ensures only Global Admins can access these endpoints.

### **A3 — Add OpenAPI annotations**
Document that these endpoints require the Global Administrator role.

---

# Backend Task Group B — Company CRUD Endpoints (Admin‑Only)

### **B1 — Create `AdminCompaniesController`**
Route: `/api/admin/companies`

### **B2 — Endpoints**
- `GET /api/admin/companies` — list all companies  
- `GET /api/admin/companies/{id}` — get details  
- `POST /api/admin/companies` — create  
- `PUT /api/admin/companies/{id}` — update  
- `DELETE /api/admin/companies/{id}` — delete  

### **B3 — Inject**
- `ICompanyRepository`  
- `IMapper`  
- `ILogger<AdminCompaniesController>`  

### **B4 — Use DTOs**
- `CompanyDto`  
- `CreateCompanyRequest`  
- `UpdateCompanyRequest`  

### **B5 — Add validation**
Use FluentValidation for create/update requests.

### **B6 — Add observability**
Wrap each endpoint in OpenTelemetry spans.

---

# Backend Task Group C — User Assignment Endpoints

### **C1 — Create `AdminCompanyUsersController`**
Route: `/api/admin/companies/{companyId}/users`

### **C2 — Endpoints**
- `GET /api/admin/companies/{companyId}/users`  
- `POST /api/admin/companies/{companyId}/users/{userId}` — assign user  
- `DELETE /api/admin/companies/{companyId}/users/{userId}` — unassign user  

### **C3 — Inject**
- `ICompanyMembershipService`  
- `IUserRepository`  
- `ICompanyRepository`  
- `ILogger<AdminCompanyUsersController>`  

### **C4 — Add validation**
- Ensure company exists  
- Ensure user exists  
- Prevent duplicate assignments  

### **C5 — Add observability**
- Log assignment/unassignment events  
- Add OTel spans  

---

# Backend Task Group D — Repository Enhancements

### **D1 — Add methods to `ICompanyMembershipService`**
- `Task AssignUserToCompanyAsync(Guid userId, Guid companyId)`  
- `Task RemoveUserFromCompanyAsync(Guid userId, Guid companyId)`  
- `Task<IReadOnlyList<AppUser>> GetUsersForCompanyAsync(Guid companyId)`  

### **D2 — Implement in Infrastructure**
Use the `CompanyUser` join table.

### **D3 — Add EF Core configurations**
Ensure composite key on `(UserId, CompanyId)`.

---

# Backend Task Group E — Admin‑Only Bypass of Global Query Filters

Global Admins must be able to see **all companies**, not just their own.

### **E1 — Add `IgnoreQueryFilters()` for admin endpoints**
In admin controllers:

```csharp
var companies = await _db.Companies
    .IgnoreQueryFilters()
    .ToListAsync();
```

### **E2 — Add helper extension**
`DbSet<T>.AsAdminQueryable(ICurrentUserService user)`  
- If Global Admin → ignore filters  
- Else → apply filters  

---

# Frontend Task Group F — Role‑Based Navigation

### **F1 — Extend `useCurrentUser()`**
Add:
- `roles: string[]`  
- `isGlobalAdmin: boolean`  

### **F2 — Add navigation item**
Only show “Companies” if `isGlobalAdmin`.

### **F3 — Add protected route**
In `App.tsx`:

```tsx
<Route
  path="/admin/companies"
  element={
    <ProtectedRoute requireGlobalAdmin>
      <CompanyAdminPage />
    </ProtectedRoute>
  }
/>
```

---

# Frontend Task Group G — API Client for Admin Endpoints

### **G1 — Create `/api/adminCompaniesApi.ts`**
Functions:
- `getCompanies()`  
- `getCompany(id)`  
- `createCompany(request)`  
- `updateCompany(id, request)`  
- `deleteCompany(id)`  

### **G2 — Create `/api/adminCompanyUsersApi.ts`**
Functions:
- `getUsersForCompany(companyId)`  
- `assignUser(companyId, userId)`  
- `unassignUser(companyId, userId)`  

### **G3 — Add Axios interceptors**
- Attach bearer token  
- Handle 403 → show “Access denied”  

---

# Frontend Task Group H — React Query Hooks

### **H1 — Company CRUD hooks**
- `useAdminCompanies()`  
- `useAdminCompany(id)`  
- `useCreateCompany()`  
- `useUpdateCompany()`  
- `useDeleteCompany()`  

### **H2 — User assignment hooks**
- `useCompanyUsers(companyId)`  
- `useAssignUser(companyId)`  
- `useUnassignUser(companyId)`  

### **H3 — Add optimistic updates**
For user assignment/unassignment.

---

# Frontend Task Group I — Admin UI Pages

### **I1 — Create `CompanyAdminPage.tsx`**
Includes:
- Table of companies  
- “Create Company” button  
- Edit/Delete actions  
- “Manage Users” button  

### **I2 — Create `CompanyForm.tsx`**
Used for create + edit.

### **I3 — Create `CompanyUsersPage.tsx`**
Includes:
- List of assigned users  
- Dropdown of available users  
- “Assign User” button  
- “Remove” buttons  

### **I4 — Add skeleton loaders + error states**

---

# Frontend Task Group J — Observability + Resilience

### **J1 — Wrap admin API calls in OTel spans**

### **J2 — Add retry/backoff for transient errors**

### **J3 — Add circuit breaker integration**
Admin pages should degrade gracefully.

### **J4 — Add toast notifications**
- Success  
- Error  
- Access denied  

---

# **End Result**

You will have:

- A secure, admin‑only company management module  
- Full CRUD for companies  
- Ability to assign/unassign users  
- Role‑based navigation and routing  
- Clean React UI with React Query  
- Observability and resilience baked in  
- Multi‑tenant rules respected everywhere  

This is a major administrative feature and fits perfectly into your existing architecture.