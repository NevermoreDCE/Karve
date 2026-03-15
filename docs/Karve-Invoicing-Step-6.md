
# ⚙️ **STEP 6 — Background Jobs, Queues, and Email Reminders**

## 🎯 Step 6 Goals
By the end of Step 6, you will have:

- A clean background processing subsystem  
- A queue abstraction using `System.Threading.Channels`  
- A hosted service that processes queued work  
- An email notification service (vendor‑neutral)  
- A scheduled job that checks for overdue invoices  
- A pattern for adding future background tasks  
- Observability integrated into background jobs  
- Tests for queueing and background processing  

This step adds real operational capability to Project Karve.

---

# 🧩 Task Group A — Create a Background Job Queue (Channel‑Based)

### **A1 — Create `/BackgroundJobs` folder in Application project**

### **A2 — Create `IBackgroundJobQueue` interface**
Methods:
- `ValueTask QueueAsync<T>(T job)`  
- `ValueTask<T> DequeueAsync(CancellationToken)`  

### **A3 — Create `BackgroundJobQueue` implementation**
Use `Channel<T>` internally:
- Unbounded channel  
- Single consumer  
- Thread‑safe  

### **A4 — Register queue in DI**
In API `Program.cs`:
```csharp
builder.Services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
```

---

# 🧩 Task Group B — Create a Hosted Background Worker

### **B1 — Create `BackgroundJobWorker` class**
In API project, inherit from `BackgroundService`.

### **B2 — Inject:**
- `IBackgroundJobQueue`  
- `ILogger<BackgroundJobWorker>`  
- `IServiceProvider` (to create scopes per job)  

### **B3 — Implement job processing loop**
- Dequeue job  
- Create DI scope  
- Resolve appropriate handler  
- Execute job  
- Wrap in try/catch  
- Log failures  
- Emit OpenTelemetry spans  

### **B4 — Register worker**
```csharp
builder.Services.AddHostedService<BackgroundJobWorker>();
```

---

# 🧩 Task Group C — Define Job Types and Handlers

### **C1 — Create `/BackgroundJobs/Jobs` folder**

### **C2 — Create job record types**
Examples:
- `SendInvoiceEmailJob(Guid invoiceId, Guid companyId)`  
- `CheckOverdueInvoicesJob(Guid companyId)`  

### **C3 — Create `/BackgroundJobs/Handlers` folder**

### **C4 — Create handler interfaces**
```csharp
public interface IBackgroundJobHandler<TJob>
{
    Task HandleAsync(TJob job, CancellationToken cancellationToken);
}
```

### **C5 — Implement handlers**
Examples:
- `SendInvoiceEmailJobHandler`  
- `CheckOverdueInvoicesJobHandler`  

### **C6 — Register handlers in DI**
Use:
```csharp
builder.Services.AddScoped<IBackgroundJobHandler<SendInvoiceEmailJob>, SendInvoiceEmailJobHandler>();
```

---

# 🧩 Task Group D — Add Email Sending (Vendor‑Neutral)

### **D1 — Create `/Email` folder in Application project**

### **D2 — Create `IEmailSender` interface**
Methods:
- `Task SendAsync(EmailMessage message)`  

### **D3 — Create `EmailMessage` model**
Properties:
- To  
- Subject  
- Body (HTML or text)  

### **D4 — Create vendor‑neutral implementation**
In Infrastructure:
- Use `SmtpClient` or a simple HTTP‑based provider  
- No vendor lock‑in  

### **D5 — Add `EmailOptions` class**
Properties:
- SMTP host  
- Port  
- Username  
- Password  
- From address  

### **D6 — Bind options in `Program.cs`**

---

# 🧩 Task Group E — Add Scheduled Jobs (Overdue Invoice Checker)

### **E1 — Create `OverdueInvoiceScheduler` class**
Inherit from `BackgroundService`.

### **E2 — Inject:**
- `IBackgroundJobQueue`  
- `ILogger<OverdueInvoiceScheduler>`  
- `ICurrentUserService` (optional)  

### **E3 — Implement schedule loop**
Every X minutes:
- Enqueue `CheckOverdueInvoicesJob` for each company  

### **E4 — Register scheduler**
```csharp
builder.Services.AddHostedService<OverdueInvoiceScheduler>();
```

---

# 🧩 Task Group F — Integrate Background Jobs With API

### **F1 — Update Invoice creation endpoint**
After creating invoice:
- Enqueue `SendInvoiceEmailJob`  

### **F2 — Update Payment creation endpoint**
After payment:
- Optionally enqueue a “payment received” email job  

### **F3 — Add endpoint to manually trigger overdue check**
For admin use.

---

# 🧩 Task Group G — Add Observability to Background Jobs

### **G1 — Add OpenTelemetry spans in job handlers**
Example:
```csharp
using var span = tracer.StartActiveSpan("SendInvoiceEmailJob");
```

### **G2 — Add structured logs**
Include:
- Job type  
- CompanyId  
- InvoiceId  

### **G3 — Add correlation IDs**
Generate new correlation ID per job.

---

# 🧩 Task Group H — Add Tests for Background Processing

### **H1 — Add tests for `BackgroundJobQueue`**
- Queue/dequeue behavior  
- Thread safety  

### **H2 — Add tests for job handlers**
Use in‑memory DbContext.

### **H3 — Add tests for scheduler**
Mock time using `TestClock` pattern.

### **H4 — Add integration test**
- Enqueue job  
- Worker processes job  
- Assert side effects  

---

# 🧩 Task Group I — Add UI Hooks (Optional for Step 6)

### **I1 — Add “Send Invoice Email” button**
Calls API → enqueues job.

### **I2 — Add “Run Overdue Check” button**
Admin‑only.

### **I3 — Add toast notifications**
Show success/failure.

---

# 🎉 Step 6 Complete
Once you finish these tasks, you will have:

- A robust background processing system  
- A queue abstraction using Channels  
- Hosted services for job execution and scheduling  
- Email notifications  
- Overdue invoice automation  
- Observability integrated into background jobs  
- Tests for all background components  

This is a major milestone — your system now has real operational capabilities.

