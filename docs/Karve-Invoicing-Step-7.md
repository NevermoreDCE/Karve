
# ⚡ **STEP 7 — Caching (In‑Memory, Output Caching, EF Caching, React Query)**  
### *Vendor‑neutral, Redis‑optional, production‑ready caching patterns*

## 🎯 Step 7 Goals
By the end of Step 7, you will have:

- In‑memory caching for hot data  
- Output caching for high‑traffic GET endpoints  
- Optional EF Core second‑level caching  
- Cache‑aside patterns in repositories  
- Cache invalidation on writes  
- Background cache warmers  
- React Query caching tuned for multi‑tenant data  
- Observability for cache hits/misses  

This gives Project Karve a serious performance boost without committing to Redis or any external cache.

---

# 🧩 **Task Group A — Add In‑Memory Caching (IMemoryCache)**

### **A1 — Add IMemoryCache to API**
In `Program.cs`:

```csharp
builder.Services.AddMemoryCache();
```

### **A2 — Create `/Caching` folder in Application project**

### **A3 — Create `ICacheService` interface**
Methods:
- `Task<T?> GetAsync<T>(string key)`
- `Task SetAsync<T>(string key, T value, TimeSpan ttl)`
- `Task RemoveAsync(string key)`

### **A4 — Implement `MemoryCacheService` in Infrastructure**
Use `IMemoryCache` internally.

### **A5 — Register implementation**
```csharp
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

---

# 🧩 **Task Group B — Add Cache Keys + Naming Conventions**

### **B1 — Create `CacheKeys` static class**
Examples:
- `GetProductList(companyId)`  
- `GetCustomerList(companyId)`  
- `GetInvoiceSummary(companyId)`  

### **B2 — Add helper methods for multi‑tenant keys**
Include company ID in every key.

---

# 🧩 **Task Group C — Add Cache‑Aside Pattern to Repositories**

### **C1 — Update ProductRepository**
Wrap read operations:

```csharp
var key = CacheKeys.GetProductList(companyId);
var cached = await cache.GetAsync<List<ProductDto>>(key);
if (cached != null) return cached;

var products = await db.Products.Where(...).ToListAsync();
await cache.SetAsync(key, products, TimeSpan.FromMinutes(10));
return products;
```

### **C2 — Add invalidation on writes**
After create/update/delete:

```csharp
await cache.RemoveAsync(CacheKeys.GetProductList(companyId));
```

### **C3 — Repeat for:**
- Customers  
- Invoices (summary lists)  
- Company metadata  

### **C4 — Add observability**
Log cache hits/misses.

---

# 🧩 **Task Group D — Add Output Caching (ASP.NET Core OutputCache)**

### **D1 — Add OutputCache middleware**
In `Program.cs`:

```csharp
builder.Services.AddOutputCache();
app.UseOutputCache();
```

### **D2 — Add output caching to controllers**
Example:

```csharp
[OutputCache(Duration = 60, VaryByQueryKeys = new[] { "page", "pageSize" })]
[HttpGet]
public async Task<IActionResult> GetInvoices(...)
```

### **D3 — Add multi‑tenant variation**
Use:

```csharp
VaryByHeader = "X-Company-Id"
```

### **D4 — Apply to:**
- GET /products  
- GET /customers  
- GET /invoices  
- GET /companies/{id}  

---

# 🧩 **Task Group E — Optional: Add EF Core Second‑Level Cache**

### **E1 — Add package**
```bash
dotnet add src/Karve.Invoicing.Infrastructure package EFCoreSecondLevelCacheInterceptor
```

### **E2 — Register in Infrastructure**
Add:

```csharp
services.AddEFSecondLevelCache(options =>
{
    options.UseMemoryCacheProvider();
});
```

### **E3 — Add interceptor to DbContext**
In `AddDbContext`:

```csharp
options.AddInterceptors(new SecondLevelCacheInterceptor());
```

### **E4 — Mark queries as cacheable**
In repositories:

```csharp
var result = await query.Cacheable().ToListAsync();
```

---

# 🧩 **Task Group F — Add Background Cache Warmers**

### **F1 — Create `CacheWarmJob` record**
Example:
```csharp
public record WarmProductCacheJob(Guid CompanyId);
```

### **F2 — Create handler**
`WarmProductCacheJobHandler`:
- Load products from DB  
- Store in cache  

### **F3 — Add scheduler**
Every 15 minutes:
- Enqueue warmers for each company  

### **F4 — Add observability**
Log warm events.

---

# 🧩 **Task Group G — Add HTTP Client Caching (Polly)**

### **G1 — Add Polly caching package**
```bash
dotnet add src/Karve.Invoicing.Api package Polly.Extensions.Http
```

### **G2 — Add in‑memory cache policy**
```csharp
var cachePolicy = Policy.CacheAsync(
    new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
    TimeSpan.FromMinutes(5));
```

### **G3 — Apply to outbound HTTP clients**
Useful for:
- Tax rate APIs  
- Currency conversion  
- Email provider metadata  

---

# 🧩 **Task Group H — Add Frontend Caching (React Query)**

### **H1 — Tune React Query defaults**
In `queryClient.ts`:

```ts
defaultOptions: {
  queries: {
    staleTime: 60_000,
    cacheTime: 300_000,
    refetchOnWindowFocus: false,
  }
}
```

### **H2 — Add multi‑tenant query keys**
Example:

```ts
useQuery(['invoices', companyId], ...)
```

### **H3 — Add optimistic updates**
For:
- Invoice creation  
- Customer updates  
- Product updates  

### **H4 — Add cache invalidation on mutations**
Example:

```ts
queryClient.invalidateQueries(['invoices', companyId]);
```

---

# 🧩 **Task Group I — Add Observability for Caching**

### **I1 — Add logs for cache hits/misses**
In `MemoryCacheService`.

### **I2 — Add OpenTelemetry spans**
Wrap cache operations:

```csharp
using var span = tracer.StartActiveSpan("cache.get");
```

### **I3 — Add metrics**
- Cache hit count  
- Cache miss count  
- Cache warm count  

---

# 🎉 **Step 7 Complete**
Once you finish these tasks, you will have:

- A robust, vendor‑neutral caching layer  
- Output caching for high‑traffic endpoints  
- Cache‑aside patterns in repositories  
- EF Core second‑level caching (optional)  
- Background cache warmers  
- React Query caching tuned for multi‑tenant data  
- Observability for cache behavior  

And you’ll be able to add Redis later with almost no code changes.

