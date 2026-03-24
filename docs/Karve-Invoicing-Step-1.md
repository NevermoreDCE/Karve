
# 🧱 **STEP 1 — Project Setup + Domain + EF Core + SQLite**  
> Verification status updated on 2026-03-22.

## **Task Group A — Create Solution + Project Structure**

### **A1 — Create the solution and folder structure** [DONE]
Create the following folder structure:

```
/src
  /Karve.Invoicing.Domain
  /Karve.Invoicing.Application
  /Karve.Invoicing.Infrastructure
  /Karve.Invoicing.Api

/tests
  /Karve.Invoicing.Domain.Tests
  /Karve.Invoicing.Application.Tests
  /Karve.Invoicing.Api.Tests
```

### **A2 — Create the solution** [DONE]
```
dotnet new sln -n Karve.Invoicing
```

### **A3 — Create the four main projects** [DONE]
```
dotnet new classlib -n Karve.Invoicing.Domain -o src/Karve.Invoicing.Domain
dotnet new classlib -n Karve.Invoicing.Application -o src/Karve.Invoicing.Application
dotnet new classlib -n Karve.Invoicing.Infrastructure -o src/Karve.Invoicing.Infrastructure
dotnet new webapi  -n Karve.Invoicing.Api -o src/Karve.Invoicing.Api
```

### **A4 — Add projects to the solution** [DONE]
```
dotnet sln add src/Karve.Invoicing.Domain
dotnet sln add src/Karve.Invoicing.Application
dotnet sln add src/Karve.Invoicing.Infrastructure
dotnet sln add src/Karve.Invoicing.Api
```

### **A5 — Add project references** [DONE]
- API → Application  
- Application → Domain  
- Infrastructure → Application + Domain  
- API → Infrastructure  

```
dotnet add src/Karve.Invoicing.Application reference src/Karve.Invoicing.Domain
dotnet add src/Karve.Invoicing.Infrastructure reference src/Karve.Invoicing.Application
dotnet add src/Karve.Invoicing.Infrastructure reference src/Karve.Invoicing.Domain
dotnet add src/Karve.Invoicing.Api reference src/Karve.Invoicing.Infrastructure
```

---

## **Task Group B — Add Required NuGet Packages**

### **B1 — Domain project packages** [DONE]
None required yet.

### **B2 — Application project packages** [DONE]
```
dotnet add src/Karve.Invoicing.Application package Microsoft.Extensions.DependencyInjection.Abstractions
```

### **B3 — Infrastructure project packages** [DONE]
```
dotnet add src/Karve.Invoicing.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/Karve.Invoicing.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/Karve.Invoicing.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/Karve.Invoicing.Infrastructure package Microsoft.Extensions.Configuration
dotnet add src/Karve.Invoicing.Infrastructure package Microsoft.Extensions.Configuration.Abstractions
```

### **B4 — API project packages** [DONE]
```
dotnet add src/Karve.Invoicing.Api package Microsoft.EntityFrameworkCore
dotnet add src/Karve.Invoicing.Api package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/Karve.Invoicing.Api package Swashbuckle.AspNetCore
```

---

# 🧩 **Task Group C — Domain Layer (Entities + Value Objects + Enums)**

### **C1 — Create the `Company` entity** [DONE]
Properties:
- Id (Guid)
- Name (string)
- Navigation collections: Users, Customers, Products, Invoices, Payments

### **C2 — Create the `AppUser` entity** [DONE]
Properties:
- Id (Guid)
- ExternalUserId (string)
- Email (EmailAddress VO)
- Navigation: CompanyUsers (many‑to‑many)

### **C3 — Create the `CompanyUser` join entity** [DONE]
Properties:
- CompanyId
- UserId
- Role (enum or string)

### **C4 — Create the `Customer` entity** [DONE]
Properties:
- Id
- CompanyId
- Name
- Email (EmailAddress VO)
- BillingAddress (string)
- Navigation: Invoices

### **C5 — Create the `Product` entity** [DONE]
Properties:
- Id
- CompanyId
- Name
- Sku
- UnitPrice (Money VO)

### **C6 — Create the `Invoice` entity** [DONE]
Properties:
- Id
- CompanyId
- CustomerId
- InvoiceDate
- DueDate
- Status (InvoiceStatus enum)
- Navigation: Customer, LineItems, Payments

### **C7 — Create the `InvoiceLineItem` entity** [DONE]
Properties:
- Id
- InvoiceId
- ProductId
- Quantity
- UnitPrice (Money VO)

### **C8 — Create the `Payment` entity** [DONE]
Properties:
- Id
- CompanyId
- InvoiceId
- Amount (Money VO)
- PaymentDate
- Method (PaymentMethod enum)

### **C9 — Create value objects** [DONE]
- `Money` (decimal Amount, string Currency)
- `EmailAddress` (string Value, validation in constructor)

### **C10 — Create enums** [DONE]
- `InvoiceStatus` (Draft, Sent, Viewed, Paid, Overdue, Canceled)
- `PaymentMethod` (CreditCard, ACH, Check, Cash, Other)
- `UserRole` (Admin, User, Viewer)

---

# 🧱 **Task Group D — Application Layer (Interfaces Only for Now)**

### **D1 — Create repository interfaces** [DONE]
In `Karve.Invoicing.Application/Interfaces`:

- `ICompanyRepository`
- `IUserRepository`
- `ICustomerRepository`
- `IProductRepository`
- `IInvoiceRepository`
- `IPaymentRepository`

Each interface should include:
- `Task<T?> GetByIdAsync(Guid id)`
- `Task AddAsync(T entity)`
- `Task UpdateAsync(T entity)`
- `Task DeleteAsync(T entity)`
- Query methods as needed (e.g., GetByCompanyIdAsync)

### **D2 — Create service interfaces (optional for Step 1)** [DONE]
- `IInvoiceService`
- `ICustomerService`
- `IProductService`

Keep them empty or minimal for now.

---

# 🧩 **Task Group E — Infrastructure Layer (EF Core + SQLite)**

### **E1 — Create the `InvoicingDbContext`** [DONE]
- Inherit from `DbContext`
- Add DbSets for all entities
- Add constructor accepting `DbContextOptions<InvoicingDbContext>`

### **E2 — Configure entity relationships using Fluent API** [DONE]
In `OnModelCreating`:
- Many‑to‑many: CompanyUser
- One‑to‑many: Company → Customers, Products, Invoices, Payments
- One‑to‑many: Customer → Invoices
- One‑to‑many: Invoice → LineItems, Payments
- One‑to‑many: Product → LineItems

### **E3 — Configure value objects** [DONE]
- Money → owned type
- EmailAddress → owned type

### **E4 — Configure EF Core conventions** [DONE]
- Use snake_case or camelCase table names (your choice)
- Configure decimal precision for Money.Amount
- Configure required fields

### **E5 — Add SQLite configuration** [DONE]
In Infrastructure:
- Add extension method `AddInvoicingDbContext`  
- Configure SQLite connection string  
- Register DbContext with DI  

### **E6 — Implement repository classes** [DONE]
For each interface in Application:
- Create EF Core implementation in Infrastructure  
- Use DbContext  
- Use async methods  
- No business logic yet  

---

# 🧱 **Task Group F — API Layer (Minimal Setup)**

### **F1 — Configure Program.cs** [DONE]
- Add DbContext registration (via Infrastructure extension)
- Add OpenAPI and Scalar
- Add minimal CORS policy (allow localhost:3000)
- Add health check endpoint `/health`

### **F2 — Create placeholder controllers** [DONE]
- `InvoicesController`
- `CustomersController`
- `ProductsController`
- `CompaniesController`
- `PaymentsController`

Each should have:
- `[ApiController]`
- `[Route("api/[controller]")]`
- A simple GET endpoint returning `Ok("placeholder")`

---

# 🧪 **Task Group G — Tests**

### **G1 — Create test projects** [DONE]
```
dotnet new xunit -n Karve.Invoicing.Domain.Tests -o tests/Karve.Invoicing.Domain.Tests
dotnet new xunit -n Karve.Invoicing.Application.Tests -o tests/Karve.Invoicing.Application.Tests
dotnet new xunit -n Karve.Invoicing.Api.Tests -o tests/Karve.Invoicing.Api.Tests
```

### **G2 — Add references** [DONE]
Each test project should reference the project it tests.

### **G3 — Add first domain tests** [DONE]
- Test `EmailAddress` validation  
- Test `Money` cannot be negative  
- Test `Invoice` cannot have negative quantities  

---

# 🧱 **Task Group H — Initial Migration**

### **H1 — Add initial migration** [DONE]
From the Infrastructure project directory:

```
dotnet ef migrations add InitialCreate -s ../Karve.Invoicing.Api
```

### **H2 — Apply migration** [DONE]
```
dotnet ef database update -s ../Karve.Invoicing.Api
```

---

# 🎉 Step 1 Complete  
Once you finish these tasks, you will have:

- A clean Onion Architecture  
- A complete domain model  
- EF Core configured with SQLite  
- A working DbContext  
- Repository interfaces + implementations  
- A functioning API shell  
- Initial migration + database  
- First unit tests  

This is the perfect foundation for Step 2 (API endpoints + DTOs + AutoMapper).

---
