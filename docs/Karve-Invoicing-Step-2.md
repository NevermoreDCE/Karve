

# 🌟 Step 2 — Build the API Surface (DTOs, Mapping, Validation, Controllers)

> Verification status updated on 2026-03-22.

## 🎯 Step 2 Goals
By the end of Step 2, you will have:

- A clean DTO layer  
- AutoMapper profiles  
- FluentValidation validators  
- Fully implemented CRUD endpoints for all major entities  
- Repository usage wired into controllers  
- Proper error handling and response types  
- Modern .NET 10 API conventions  
- A consistent API shape ready for the React frontend  

---

# 🧩 Task Group A — Add DTOs (Data Transfer Objects)

### **A1 — Create a `/DTOs` folder in the Application project** [DONE]
Inside `src/Karve.Invoicing.Application/DTOs`.

### **A2 — Create DTOs for Company** [DONE]
- `CompanyDto`
- `CreateCompanyRequest`
- `UpdateCompanyRequest`

### **A3 — Create DTOs for User** [DONE]
- `UserDto`
- `CreateUserRequest`
- `UpdateUserRequest`

### **A4 — Create DTOs for Customer** [DONE]
- `CustomerDto`
- `CreateCustomerRequest`
- `UpdateCustomerRequest`

### **A5 — Create DTOs for Product** [DONE]
- `ProductDto`
- `CreateProductRequest`
- `UpdateProductRequest`

### **A6 — Create DTOs for Invoice** [DONE]
- `InvoiceDto`
- `CreateInvoiceRequest`
- `UpdateInvoiceRequest`

### **A7 — Create DTOs for InvoiceLineItem** [DONE]
- `InvoiceLineItemDto`
- `CreateInvoiceLineItemRequest`
- `UpdateInvoiceLineItemRequest`

### **A8 — Create DTOs for Payment** [DONE]
- `PaymentDto`
- `CreatePaymentRequest`
- `UpdatePaymentRequest`

### **A9 — Ensure DTOs use primitive types only** [DONE]
- No domain entities  
- No value objects  
- Flatten Money/EmailAddress into simple fields (e.g., `decimal Amount`, `string Email`)

---

# 🧩 Task Group B — Add AutoMapper Profiles

### **B1 — Add AutoMapper package to Application project** [DONE]
```
dotnet add src/Karve.Invoicing.Application package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### **B2 — Create `/Mapping` folder in Application project** [DONE]

### **B3 — Create `DomainToDtoProfile`** [DONE]
Maps:
- Company → CompanyDto  
- Customer → CustomerDto  
- Product → ProductDto  
- Invoice → InvoiceDto  
- InvoiceLineItem → InvoiceLineItemDto  
- Payment → PaymentDto  
- User → UserDto  

### **B4 — Create `DtoToDomainProfile`** [DONE]
Maps:
- CreateXRequest → Entity  
- UpdateXRequest → Entity  

### **B5 — Register AutoMapper in API** [DONE]
In `Program.cs`:
```csharp
builder.Services.AddAutoMapper(typeof(Karve.Invoicing.Application.AssemblyMarker));
```

### **B6 — Add an `AssemblyMarker` class** [DONE]
In Application project root:
```csharp
public sealed class AssemblyMarker { }
```

---

# 🧩 Task Group C — Add FluentValidation

### **C1 — Add FluentValidation package** [DONE]
```
dotnet add src/Karve.Invoicing.Application package FluentValidation
dotnet add src/Karve.Invoicing.Api package FluentValidation.AspNetCore
```

### **C2 — Create `/Validators` folder in Application project** [DONE]

### **C3 — Create validators for each Create/Update request** [DONE]
Examples:
- `CreateCustomerRequestValidator`
- `UpdateCustomerRequestValidator`
- `CreateInvoiceRequestValidator`
- etc.

### **C4 — Register FluentValidation in API** [DONE]
In `Program.cs`:
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();
```

---

# 🧩 Task Group D — Add API Response Models

### **D1 — Create `/Responses` folder in Application project** [DONE]

### **D2 — Create `ApiResponse<T>`** [DONE]
Properties:
- `bool Success`
- `T? Data`
- `string? Error`

### **D3 — Create helper methods** [DONE]
- `ApiResponse.Success(data)`
- `ApiResponse.Failure(errorMessage)`

---

# 🧩 Task Group E — Implement Controllers (Full CRUD)

For each controller, follow this pattern:

- Inject repository + mapper  
- Validate incoming DTOs  
- Map DTO → entity  
- Save via repository  
- Map entity → DTO  
- Return `ApiResponse<T>`  

### **E1 — Implement CompaniesController** [DONE]
Endpoints:
- GET /api/companies  
- GET /api/companies/{id}  
- POST /api/companies  
- PUT /api/companies/{id}  
- DELETE /api/companies/{id}  

### **E2 — Implement UsersController** [DONE]
Same CRUD pattern.

### **E3 — Implement CustomersController** [DONE]

### **E4 — Implement ProductsController** [DONE]

### **E5 — Implement InvoicesController** [BACKLOG]
Not yet implemented:
- `POST /api/invoices/{id}/line-items` currently returns `501 Not Implemented`.
- `POST /api/invoices/{id}/payments` currently returns `501 Not Implemented`.
- Create workflows for nested invoice line items and nested invoice payments are not wired to persistence yet.

Additional endpoints:
- GET /api/invoices/{id}/line-items  
- POST /api/invoices/{id}/line-items  
- GET /api/invoices/{id}/payments  
- POST /api/invoices/{id}/payments  

### **E6 — Implement PaymentsController** [DONE]

### **E7 — Ensure all controllers return `ApiResponse<T>`** [DONE]

---

# 🧩 Task Group F — Add Error Handling Middleware

### **F1 — Create `/Middleware` folder in API project** [DONE]

### **F2 — Create `ExceptionHandlingMiddleware`** [DONE]
- Catch all exceptions  
- Log exception  
- Return `ApiResponse.Failure("...")`  

### **F3 — Register middleware** [DONE]
In `Program.cs`:
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

# 🧩 Task Group G — Add Pagination + Filtering Support

### **G1 — Create `PagedResult<T>` model** [DONE]
Properties:
- Items  
- TotalCount  
- Page  
- PageSize  

### **G2 — Add pagination parameters to GET endpoints** [DONE]
- `int page = 1`  
- `int pageSize = 20`  

### **G3 — Add filtering for company‑scoped queries** [DONE]
(even though global filters come later)

---

# 🧩 Task Group H — Add API Conventions + Versioning

### **H1 — Add API versioning package** [DONE]
```
dotnet add src/Karve.Invoicing.Api package Microsoft.AspNetCore.Mvc.Versioning
```

### **H2 — Configure versioning** [DONE]
- Default version = 1.0  
- Assume default version when unspecified  

### **H3 — Add `[ApiVersion("1.0")]` to controllers** [DONE]

---

# 🧩 Task Group I — Add OpenAPI + Scalar UI Enhancements

### **I1 — Add XML comments to all controllers and DTOs** [BACKLOG]
Not yet implemented:
- XML summary comments are still missing in some DTO files.
- `UserDtos.cs` currently does not include XML summary comments on its DTO/request types.

### **I2 — Enable XML comments in API project** [DONE]
In `.csproj`:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

### **I3 — Configure Scalar to read XML comments** [DONE]
Update Scalar config to include:
```csharp
options.IncludeXmlComments = true;
```

---

# 🧪 Task Group J — Add Tests for Controllers + Mapping + Validation

### **J1 — Add mapping tests** [DONE]
- Ensure DTO → entity → DTO round‑trips correctly  

### **J2 — Add validator tests** [DONE]
- Invalid DTOs fail  
- Valid DTOs pass  

### **J3 — Add controller tests using WebApplicationFactory** [DONE]
- GET returns expected data  
- POST creates entity  
- PUT updates entity  
- DELETE removes entity  

---

# 🎉 Step 2 Complete  
Once you finish these tasks, you will have:

- A fully functional, well‑structured API  
- DTOs, validators, mappings, and controllers  
- Clean error handling  
- Pagination + filtering  
- Versioning  
- A polished OpenAPI UI  
- Tests for the entire API surface  

This is the perfect launchpad for **Step 3: Authentication + Multi‑Tenant Enforcement + Global Query Filters**.

