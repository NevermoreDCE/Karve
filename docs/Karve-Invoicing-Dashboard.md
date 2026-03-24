**Planning dashboard metrics**

I need to come up with a plan for dashboard metrics that are precomputed and cached through background jobs. The user wants a detailed breakdown of tasks for both the backend and frontend. I should keep the answer concise but still detailed enough to cover everything. Maybe I’ll organize it with headings and lists to make it clearer. Let’s get started on outlining the steps for both parts of the process.
Great choice—Option B leans into all the machinery you’ve already built: background jobs, caching, observability. Let’s treat this as **Step 9 – Dashboard Metrics (Pre‑computed + Cached)** and break it into small, Copilot‑friendly tasks.

I’ll assume:

- Metrics are **company‑scoped**  
- Pre‑computation happens via **background jobs**  
- Results are stored in a **DashboardMetrics** table and/or cache  
- API exposes a **single dashboard endpoint** per company  
- React consumes that endpoint via **React Query** and renders 12 widgets  

---

## 🧱 Step 9 – High‑Level Shape

- **Backend:**
  - Domain model + DTOs for dashboard metrics
  - Background job to compute metrics per company
  - Repository/service to read/write metrics
  - API endpoint: `GET /api/dashboard`
  - Caching + observability

- **Frontend:**
  - TypeScript types for dashboard response
  - React Query hook: `useDashboard()`
  - 12 components (one per widget)
  - Dashboard layout page

---

## 🧩 Backend Task Group A — Domain + DTOs for Dashboard

**A1 — Create `Dashboard` folder in Application project**

- Path: `src/Karve.Invoicing.Application/Dashboard`

**A2 — Create `DashboardMetrics` domain model (or read model)**

Include properties (per company):

- `Guid CompanyId`
- `decimal TotalOutstandingAmount`
- `decimal TotalOverdueAmount`
- `decimal InvoicesDueSoonAmount`
- `decimal PaymentsReceivedThisMonth`
- `IReadOnlyList<MonthlyRevenuePoint> RevenueTrend`
- `InvoiceStatusBreakdown StatusBreakdown`
- `double AveragePaymentDays`
- `IReadOnlyList<TopCustomerDto> TopCustomers`
- `IReadOnlyList<ActivityItemDto> RecentActivity`
- `IReadOnlyList<UnpaidInvoiceDto> TopUnpaidInvoices`
- `int NewCustomersThisMonth`
- `IReadOnlyList<ProductRevenueDto> ProductRevenueBreakdown`
- `DateTimeOffset LastCalculatedAt`

**A3 — Create DTOs in Application**

- `DashboardMetricsDto`
- `MonthlyRevenuePointDto { Date Month; decimal Amount }`
- `InvoiceStatusBreakdownDto { Draft, Sent, Viewed, PartiallyPaid, Paid, Overdue }`
- `TopCustomerDto { Guid CustomerId; string Name; decimal TotalRevenue }`
- `ActivityItemDto { DateTimeOffset Timestamp; string Type; string Description; Guid? InvoiceId; Guid? CustomerId }`
- `UnpaidInvoiceDto { Guid InvoiceId; string InvoiceNumber; string CustomerName; decimal Amount; int DaysOverdue }`
- `ProductRevenueDto { Guid ProductId; string Name; decimal TotalRevenue }`

**A4 — Add AutoMapper profile**

- Map domain/read model → `DashboardMetricsDto` and nested DTOs.

---

## 🧩 Backend Task Group B — Persistence for Dashboard Metrics

**B1 — Add `DashboardMetrics` entity to Infrastructure**

- Map to table `DashboardMetrics` (per company).
- Store serialized JSON for complex collections (RevenueTrend, TopCustomers, etc.) or use separate tables if you prefer.

**B2 — Update `InvoicingDbContext`**

- Add `DbSet<DashboardMetricsEntity> DashboardMetrics`.
- Configure entity in `OnModelCreating`.

**B3 — Create `IDashboardMetricsRepository`**

Methods:

- `Task<DashboardMetrics?> GetByCompanyIdAsync(Guid companyId, CancellationToken)`
- `Task UpsertAsync(DashboardMetrics metrics, CancellationToken)`

**B4 — Implement `DashboardMetricsRepository`**

- Use EF Core.
- Handle insert/update logic.

---

## 🧩 Backend Task Group C — Background Job to Compute Metrics

**C1 — Create job record**

- `ComputeDashboardMetricsJob(Guid CompanyId)`

**C2 — Create `IDashboardMetricsCalculator` service**

Methods:

- `Task<DashboardMetrics> ComputeAsync(Guid companyId, CancellationToken)`

**C3 — Implement `DashboardMetricsCalculator`**

Use EF queries to compute:

1. **Total Outstanding Amount**  
   - Sum of `Invoice.TotalAmount - Payments.Sum(Amount)` for non‑fully‑paid invoices.

2. **Overdue Amount**  
   - Same as above, but `DueDate < Today`.

3. **Invoices Due Soon (Next 7 Days)**  
   - Sum of outstanding amounts where `DueDate` between `Today` and `Today + 7`.

4. **Payments Received This Month**  
   - Sum of `Payment.Amount` where `PaymentDate` is in current month.

5. **Revenue Trend (Last 6–12 Months)**  
   - Group invoices by month, sum paid amounts.

6. **Invoice Status Breakdown**  
   - Count invoices by status.

7. **Average Payment Time (DSO)**  
   - Average `(PaymentDate - InvoiceDate)` for fully paid invoices.

8. **Top Customers by Revenue**  
   - Group by customer, sum paid amounts, take top N.

9. **Recent Activity Feed**  
   - Combine:
     - Invoice created/sent/viewed
     - Payment created
     - Customer created  
   - Order by timestamp desc, take last N.

10. **Unpaid Invoices List (Top 5)**  
    - Order unpaid invoices by `DueDate` ascending / `DaysOverdue` desc.

11. **New Customers Added This Month**  
    - Count customers where `CreatedAt` in current month.

12. **Product/Service Revenue Breakdown**  
    - Group invoice line items by product, sum line totals.

**C4 — Create `ComputeDashboardMetricsJobHandler`**

- Implements `IBackgroundJobHandler<ComputeDashboardMetricsJob>`.
- Calls `IDashboardMetricsCalculator.ComputeAsync`.
- Calls `IDashboardMetricsRepository.UpsertAsync`.

**C5 — Add observability**

- Wrap compute in OpenTelemetry span.
- Log duration and companyId.

**C6 — Register handler in DI**

---

## 🧩 Backend Task Group D — Scheduling Dashboard Recalculation

**D1 — Create `DashboardMetricsScheduler` BackgroundService**

Responsibilities:

- On interval (e.g., every 15 minutes):
  - Query all active companies.
  - Enqueue `ComputeDashboardMetricsJob` for each.

**D2 — Register scheduler**

```csharp
builder.Services.AddHostedService<DashboardMetricsScheduler>();
```

**D3 — Optional: Trigger recompute on key events**

- After invoice created/updated/paid:
  - Enqueue `ComputeDashboardMetricsJob` for that company (debounced if needed).

---

## 🧩 Backend Task Group E — Caching Layer for Dashboard

**E1 — Use existing `ICacheService`**

- Add key helper: `CacheKeys.DashboardMetrics(companyId)`.

**E2 — Create `IDashboardService`**

Methods:

- `Task<DashboardMetricsDto> GetDashboardAsync(Guid companyId, CancellationToken)`

**E3 — Implement `DashboardService`**

Flow:

1. Try cache:
   - `cache.GetAsync<DashboardMetricsDto>(key)`
2. If cache miss:
   - Load from `IDashboardMetricsRepository`.
   - If null, optionally trigger compute job and return placeholder.
   - Cache result with TTL (e.g., 5–10 minutes).
3. Return DTO.

**E4 — Add observability**

- Log cache hits/misses.
- Add OTel spans.

---

## 🧩 Backend Task Group F — API Endpoint

**F1 — Create `DashboardController`**

Route: `GET /api/dashboard`

**F2 — Inject:**

- `IDashboardService`
- `ICurrentUserService`

**F3 — Implement `GetDashboard`**

- Resolve `companyId` from `ICurrentUserService` (single or selected).
- Call `GetDashboardAsync(companyId)`.
- Return `ApiResponse<DashboardMetricsDto>`.

**F4 — Add `[Authorize]` and version attributes**

**F5 — Add OpenAPI annotations / XML comments**

---

## 🧩 Frontend Task Group G — Types + API Client

**G1 — Create `DashboardTypes.ts` in `/types`**

Mirror `DashboardMetricsDto`:

- `DashboardMetrics`
- `MonthlyRevenuePoint`
- `InvoiceStatusBreakdown`
- `TopCustomer`
- `ActivityItem`
- `UnpaidInvoice`
- `ProductRevenue`

**G2 — Add `getDashboard` API function**

In `/api/dashboardApi.ts`:

```ts
export async function getDashboard(): Promise<DashboardMetrics> {
  const response = await apiClient.get('/dashboard');
  return response.data.data;
}
```

(Assuming `ApiResponse<T>` shape.)

---

## 🧩 Frontend Task Group H — React Query Hook

**H1 — Create `useDashboard.ts` in `/hooks`**

```ts
export function useDashboard(companyId: string | undefined) {
  return useQuery({
    queryKey: ['dashboard', companyId],
    queryFn: () => getDashboard(),
    enabled: !!companyId,
    staleTime: 60_000,
  });
}
```

**H2 — Add error + loading handling**

Return `data`, `isLoading`, `isError`, etc.

---

## 🧩 Frontend Task Group I — Dashboard Page Layout

**I1 — Create `DashboardPage.tsx` in `/pages`**

- Use `useCurrentUser()` to get selected company.
- Use `useDashboard(companyId)` to fetch data.
- Layout 12 widgets in a responsive grid.

**I2 — Add route**

In `App.tsx`:

```tsx
<Route path="/dashboard" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
```

---

## 🧩 Frontend Task Group J — Individual Widgets (12 Components)

Each widget gets its own small component in `/components/dashboard`.

**J1 — `TotalOutstandingCard.tsx`**

- Props: `amount: number`
- Display big number, currency formatted.

**J2 — `OverdueAmountCard.tsx`**

- Props: `amount: number`
- Red styling if > 0.

**J3 — `InvoicesDueSoonCard.tsx`**

- Props: `amount: number`
- “Due in next 7 days”.

**J4 — `PaymentsThisMonthCard.tsx`**

- Props: `amount: number`

**J5 — `RevenueTrendChart.tsx`**

- Props: `data: MonthlyRevenuePoint[]`
- Use your preferred chart lib (e.g., Recharts).

**J6 — `InvoiceStatusBreakdownChart.tsx`**

- Props: `breakdown: InvoiceStatusBreakdown`
- Pie or bar chart.

**J7 — `AveragePaymentTimeCard.tsx`**

- Props: `days: number`

**J8 — `TopCustomersList.tsx`**

- Props: `customers: TopCustomer[]`
- Show name + total revenue.

**J9 — `RecentActivityFeed.tsx`**

- Props: `items: ActivityItem[]`
- List with icons per type.

**J10 — `UnpaidInvoicesList.tsx`**

- Props: `invoices: UnpaidInvoice[]`
- Show invoice number, customer, amount, days overdue.
- Click → navigate to invoice detail.

**J11 — `NewCustomersThisMonthCard.tsx`**

- Props: `count: number`

**J12 — `ProductRevenueBreakdownChart.tsx`**

- Props: `items: ProductRevenue[]`
- Bar or donut chart.

---

## 🧩 Frontend Task Group K — UX + Resilience

**K1 — Add skeleton loaders**

- Show skeletons while `isLoading`.

**K2 — Add error state**

- Show retry button if `isError`.

**K3 — Add observability**

- Wrap dashboard load in OTel span.
- Log errors to your OTel pipeline.

---

## ✅ At the End of Step 9

You’ll have:

- Pre‑computed, cached dashboard metrics per company
- A single, clean `GET /api/dashboard` endpoint
- A React dashboard page with 12 meaningful widgets
- Background jobs keeping metrics fresh
- Observability around dashboard computation and usage
