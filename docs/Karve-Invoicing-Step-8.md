
# 🛡️ **STEP 8 — Resilience Patterns (Backend + Frontend)**  
### *Polly for .NET + React Query / Axios / Custom Patterns for the SPA*

## 🎯 Step 8 Goals
By the end of Step 8, you will have:

- A full resilience pipeline in .NET using Polly v8  
- Retry, backoff, timeout, circuit breaker, fallback, and hedging policies  
- Policy registry + named HttpClients  
- Resilience integrated with OpenTelemetry  
- Frontend equivalents of Polly patterns using React Query, Axios, and custom hooks  
- Circuit breaker state stored in Zustand  
- Global error handling and fallback UI  
- Observability for resilience events  

This step makes Project Karve production‑ready.

---

# 🧩 **Task Group A — Add Polly v8 to the Backend**

### **A1 — Add Polly packages**
```bash
dotnet add src/Karve.Invoicing.Api package Polly
dotnet add src/Karve.Invoicing.Api package Polly.Extensions
dotnet add src/Karve.Invoicing.Api package Microsoft.Extensions.Http.Polly
```

### **A2 — Create `/Resilience` folder in API project**

### **A3 — Create `ResilienceOptions` class**
Properties:
- RetryCount  
- RetryBackoff  
- TimeoutSeconds  
- CircuitBreakerFailures  
- CircuitBreakerDuration  
- HedgingEnabled  

### **A4 — Bind options in `Program.cs`**

---

# 🧩 **Task Group B — Configure Polly Policies**

### **B1 — Create `ResiliencePolicies` static class**
Add methods to build:

- Retry with exponential backoff  
- Timeout policy  
- Circuit breaker  
- Fallback policy  
- Hedging policy  
- Bulkhead isolation  

### **B2 — Register policies in DI**
Use `AddPolicyRegistry()`.

### **B3 — Add named HttpClient with policies**
Example:

```csharp
builder.Services.AddHttpClient("EmailProvider")
    .AddPolicyHandlerFromRegistry("RetryPolicy")
    .AddPolicyHandlerFromRegistry("TimeoutPolicy")
    .AddPolicyHandlerFromRegistry("CircuitBreakerPolicy");
```

### **B4 — Add OpenTelemetry instrumentation**
Wrap policy execution with spans.

---

# 🧩 **Task Group C — Add Resilience to Repository + Service Layer**

### **C1 — Wrap external calls (email, tax API, currency API)**  
Use:

```csharp
await _policyRegistry.Get<IAsyncPolicy>("RetryPolicy")
    .ExecuteAsync(() => _emailClient.SendAsync(...));
```

### **C2 — Add fallback behavior**
Return cached or default values when external services fail.

### **C3 — Add circuit breaker logging**
Log open/close/half‑open transitions.

---

# 🧩 **Task Group D — Add Resilience to Background Jobs**

### **D1 — Wrap job handlers in retry policies**

### **D2 — Add fallback behavior for non‑critical jobs**

### **D3 — Add hedging for slow external calls**
Use two providers and race them.

### **D4 — Add OpenTelemetry spans for resilience events**

---

# 🧩 **Task Group E — Add Global Error Handling Enhancements**

### **E1 — Update `ExceptionHandlingMiddleware`**
- Map transient errors → 503  
- Map circuit breaker open → 503  
- Map timeouts → 504  

### **E2 — Add structured error responses**
Include:
- Correlation ID  
- Trace ID  
- Error category  

---

# 🌐 **FRONTEND RESILIENCE (React)**  
This is where we mirror Polly patterns using React Query, Axios, Zustand, and custom hooks.

---

# 🧩 **Task Group F — Add Retry + Backoff (React Query)**

### **F1 — Add retry logic to all queries**
```ts
retry: 3,
retryDelay: attempt => Math.min(1000 * 2 ** attempt, 8000),
```

### **F2 — Add retry only for transient errors**
Check status codes 429, 503, 504.

---

# 🧩 **Task Group G — Add Timeouts (Axios)**

### **G1 — Add Axios timeout**
```ts
axios.defaults.timeout = 5000;
```

### **G2 — Add AbortController for fetch‑based calls**

---

# 🧩 **Task Group H — Add Circuit Breaker (Zustand + Custom Hook)**

### **H1 — Create `/resilience/circuitBreakerStore.ts`**
State:
- failureCount  
- isOpen  
- openedAt  

### **H2 — Create `useCircuitBreaker()` hook**
Methods:
- `recordFailure()`  
- `recordSuccess()`  
- `shouldBlockRequest()`  

### **H3 — Integrate with Axios interceptor**
If breaker is open → short‑circuit request.

### **H4 — Add cooldown logic**
After X seconds → half‑open.

---

# 🧩 **Task Group I — Add Fallback UI**

### **I1 — Add `FallbackBoundary.tsx`**
Show:
- Cached data  
- Skeleton UI  
- “Try again” button  

### **I2 — Wrap pages with fallback boundary**

---

# 🧩 **Task Group J — Add Stale‑While‑Revalidate (React Query)**

### **J1 — Tune query defaults**
```ts
staleTime: 60_000,
cacheTime: 300_000,
refetchOnWindowFocus: false,
```

### **J2 — Add background refetching**
React Query handles this automatically.

---

# 🧩 **Task Group K — Add Hedging (Parallel Requests)**

### **K1 — Create `useHedgedRequest()` hook**
Use:

```ts
Promise.race([primaryRequest, backupRequest]);
```

### **K2 — Cancel losing request with AbortController**

---

# 🧩 **Task Group L — Add Bulkhead Isolation**

### **L1 — Create request queue**
Use a simple concurrency limiter:

```ts
const semaphore = new Semaphore(5);
```

### **L2 — Wrap Axios requests in semaphore**

---

# 🧩 **Task Group M — Add Observability for Resilience**

### **M1 — Add OpenTelemetry spans for retries**
Tag:
- retry count  
- error type  

### **M2 — Add spans for circuit breaker transitions**

### **M3 — Add logs for fallback usage**

### **M4 — Add metrics**
- Retry count  
- Timeout count  
- Circuit breaker opens  
- Fallback activations  

---

# 🎉 **Step 8 Complete**
Once you finish these tasks, you will have:

- A full resilience pipeline in .NET  
- A full resilience pipeline in React  
- Circuit breakers on both sides  
- Retry/backoff everywhere  
- Timeouts everywhere  
- Fallbacks everywhere  
- Hedging for slow external calls  
- Observability for all resilience events  

This is the step where Project Karve becomes **production‑grade and failure‑tolerant**.

