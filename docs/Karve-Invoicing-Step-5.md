# 🌟 STEP 5 — Full‑Stack Observability (Vendor‑Neutral OpenTelemetry)

## 🎯 Step 5 Goals
By the end of Step 5, you will have:

- Backend tracing, metrics, and logs using **OpenTelemetry only**  
- A clean, vendor‑neutral configuration using the Options Pattern  
- Automatic instrumentation for ASP.NET Core, EF Core, HttpClient  
- Correlation IDs flowing through logs and traces  
- A frontend telemetry layer using **OpenTelemetry Web SDK** (not Elastic RUM)  
- A consistent trace ID across frontend → backend → logs  
- A simple way to switch between Elastic, LGTM, SigNoz, etc. later  

This keeps your architecture clean and future‑proof.

---

# 🧩 Task Group A — Add OpenTelemetry to the API (Vendor‑Neutral)

### **A1 — Add OpenTelemetry packages**
```bash
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Extensions.Hosting
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Instrumentation.AspNetCore
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Instrumentation.Http
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Logs
dotnet add src/Karve.Invoicing.Api package OpenTelemetry.Metrics
```

### **A2 — Create `/Observability` folder in API project**

### **A3 — Add `OpenTelemetryOptions` class**
Properties:
- `string OtlpEndpoint`  
- `string ServiceName`  
- `string Environment`  
- `bool EnableTraces`  
- `bool EnableMetrics`  
- `bool EnableLogs`  

### **A4 — Bind options in `Program.cs`**
```csharp
builder.Services.Configure<OpenTelemetryOptions>(
    builder.Configuration.GetSection("OpenTelemetry"));
```

### **A5 — Register OpenTelemetry in `Program.cs`**
Include:
- Resource builder  
- Traces  
- Metrics  
- Logs  
- OTLP exporter  

### **A6 — Enable instrumentation**
- ASP.NET Core  
- HttpClient  
- EF Core  
- Runtime metrics  

### **A7 — Add environment‑based configuration**
Examples:
- Local dev → console exporter  
- Cloud → OTLP exporter  

---

# 🧩 Task Group B — Add Correlation ID Support

### **B1 — Create `CorrelationIdMiddleware`**
Responsibilities:
- Generate correlation ID if missing  
- Add to response headers  
- Add to logging scope  

### **B2 — Register middleware early in pipeline**

### **B3 — Update logging configuration**
Include correlation ID and trace ID.

---

# 🧩 Task Group C — Add Structured Logging (Vendor‑Neutral)

### **C1 — Add Serilog packages**
```bash
dotnet add src/Karve.Invoicing.Api package Serilog.AspNetCore
dotnet add src/Karve.Invoicing.Api package Serilog.Sinks.Console
```

### **C2 — Add `SerilogOptions` class**

### **C3 — Configure Serilog in `Program.cs`**
- JSON console output  
- Include correlation ID  
- Include trace ID  

### **C4 — Replace default logging with Serilog**

---

# 🧩 Task Group D — Add Frontend Observability (Vendor‑Neutral OpenTelemetry Web SDK)

### **D1 — Install OpenTelemetry Web SDK**
```bash
npm install @opentelemetry/api
npm install @opentelemetry/sdk-trace-web
npm install @opentelemetry/exporter-trace-otlp-http
npm install @opentelemetry/instrumentation-document-load
npm install @opentelemetry/instrumentation-fetch
npm install @opentelemetry/instrumentation-xml-http-request
```

### **D2 — Create `/observability/otel.ts`**
Initialize:

- WebTracerProvider  
- OTLP HTTP exporter  
- DocumentLoad instrumentation  
- Fetch/XHR instrumentation  

### **D3 — Add service name + environment**
Use environment variables:

```
VITE_OTEL_SERVICE_NAME=karve-invoicing-ui
VITE_OTEL_EXPORTER_OTLP_ENDPOINT=https://your-otel-endpoint
VITE_ENVIRONMENT=development
```

### **D4 — Initialize OpenTelemetry in `main.tsx`**

### **D5 — Add user context after login**
```ts
provider.getTracer('default').setAttribute('user.id', userId);
```

---

# 🧩 Task Group E — Add Frontend → Backend Trace Correlation

### **E1 — Add Axios interceptor to forward W3C traceparent header**
```ts
config.headers['traceparent'] = traceHeader;
```

### **E2 — Ensure backend OpenTelemetry reads traceparent**
OTel does this automatically.

### **E3 — Verify trace continuity locally**
Use console exporter in dev.

---

# 🧩 Task Group F — Add Error Tracking (Vendor‑Neutral)

### **F1 — Add global error boundary in React**

### **F2 — Report errors as spans**
```ts
const span = tracer.startSpan('ui.error');
span.recordException(error);
span.end();
```

### **F3 — Add API error logging**
In Axios response interceptor.

---

# 🧩 Task Group G — Add Performance Monitoring

### **G1 — Add custom spans for expensive UI operations**
Example:
```ts
const span = tracer.startSpan('render.invoice.table');
```

### **G2 — Add backend spans for expensive operations**
Wrap:
- Invoice creation  
- Payment processing  
- Background jobs  

---

# 🧩 Task Group H — Add Observability Tests

### **H1 — Backend tests**
- Correlation ID middleware  
- OpenTelemetry configuration  
- OTLP exporter configuration  

### **H2 — Frontend tests**
- OTel initialization  
- Error boundary capturing  
- Traceparent header forwarding  

---

# 🎉 Step 5 Complete (Vendor‑Neutral)
Once you finish these tasks, you will have:

- Full backend observability using OpenTelemetry  
- Full frontend observability using OpenTelemetry Web SDK  
- End‑to‑end trace correlation  
- Structured logs with correlation IDs  
- A clean, vendor‑agnostic configuration  
- The ability to switch between Elastic, LGTM, SigNoz, or anything else by changing environment variables  

This is the most future‑proof way to build observability into Project Karve.

