import { type Span } from "@opentelemetry/api";
import { DocumentLoadInstrumentation } from "@opentelemetry/instrumentation-document-load";
import { FetchInstrumentation } from "@opentelemetry/instrumentation-fetch";
import { XMLHttpRequestInstrumentation } from "@opentelemetry/instrumentation-xml-http-request";
import { ExportResultCode } from "@opentelemetry/core";
import { JsonTraceSerializer } from "@opentelemetry/otlp-transformer/build/esm/trace/json";
import { resourceFromAttributes } from "@opentelemetry/resources";
import {
  BatchSpanProcessor,
  ConsoleSpanExporter,
  type ReadableSpan,
  SimpleSpanProcessor,
  StackContextManager,
  type SpanExporter,
  WebTracerProvider,
} from "@opentelemetry/sdk-trace-web";

interface TelemetryUserContext {
  userId: string | null;
  email?: string | null;
  displayName?: string | null;
}

function envValue(name: string, fallback?: string): string {
  const value = import.meta.env[name as keyof ImportMetaEnv];
  if (typeof value === "string" && value.trim().length > 0) {
    return value;
  }

  if (fallback !== undefined) {
    return fallback;
  }

  throw new Error(`Missing required environment variable: ${name}`);
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

const serviceName = envValue("VITE_OTEL_SERVICE_NAME", "karve-invoicing-ui");
const environment = envValue(
  "VITE_ENVIRONMENT",
  import.meta.env.DEV ? "development" : "production"
);
const otlpEndpoint = envValue(
  "VITE_OTEL_EXPORTER_OTLP_ENDPOINT",
  import.meta.env.DEV ? "http://localhost:4318/v1/traces" : ""
);
const apiBaseUrl = envValue("VITE_API_BASE_URL");

class JsonOtlpTraceExporter implements SpanExporter {
  private readonly endpoint: string;
  private isShutdown = false;

  constructor(endpoint: string) {
    this.endpoint = endpoint;
  }

  export(spans: ReadableSpan[], resultCallback: (result: { code: ExportResultCode; error?: Error }) => void): void {
    if (this.isShutdown) {
      resultCallback({
        code: ExportResultCode.FAILED,
        error: new Error("The JSON OTLP trace exporter has been shut down."),
      });
      return;
    }

    const body = JsonTraceSerializer.serializeRequest(spans);
    if (!body) {
      resultCallback({
        code: ExportResultCode.FAILED,
        error: new Error("Failed to serialize trace payload."),
      });
      return;
    }

    void fetch(this.endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: new TextDecoder().decode(body),
      keepalive: true,
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`OTLP exporter responded with status ${response.status}.`);
        }

        resultCallback({ code: ExportResultCode.SUCCESS });
      })
      .catch((error: unknown) => {
        resultCallback({
          code: ExportResultCode.FAILED,
          error: error instanceof Error ? error : new Error("Unknown OTLP export failure."),
        });
      });
  }

  shutdown(): Promise<void> {
    this.isShutdown = true;
    return Promise.resolve();
  }
}

const provider = new WebTracerProvider({
  resource: resourceFromAttributes({
    "service.name": serviceName,
    "deployment.environment": environment,
  }),
  spanProcessors: [
    new BatchSpanProcessor(
      new JsonOtlpTraceExporter(otlpEndpoint)
    ),
    ...(import.meta.env.DEV
      ? [new SimpleSpanProcessor(new ConsoleSpanExporter())]
      : []),
  ],
});

const tracer = provider.getTracer(serviceName);
const instrumentations = [
  new DocumentLoadInstrumentation(),
  new FetchInstrumentation({
    propagateTraceHeaderCorsUrls: [new RegExp(`^${escapeRegExp(apiBaseUrl)}`)],
    clearTimingResources: true,
  }),
  new XMLHttpRequestInstrumentation({
    propagateTraceHeaderCorsUrls: [new RegExp(`^${escapeRegExp(apiBaseUrl)}`)],
  }),
];

let initialized = false;
let currentUserContext: TelemetryUserContext | null = null;
let currentUserSignature: string | null = null;

function applyUserAttributes(span: Span): void {
  if (!currentUserContext) {
    return;
  }

  if (currentUserContext.userId) {
    span.setAttribute("user.id", currentUserContext.userId);
  }
  if (currentUserContext.email) {
    span.setAttribute("user.email", currentUserContext.email);
  }
  if (currentUserContext.displayName) {
    span.setAttribute("user.name", currentUserContext.displayName);
  }
}

function buildUserSignature(user: TelemetryUserContext | null): string | null {
  if (!user?.userId) {
    return null;
  }

  return [user.userId, user.email ?? "", user.displayName ?? ""].join("|");
}

export function initializeOpenTelemetry(): void {
  if (initialized) {
    return;
  }

  provider.register({
    contextManager: new StackContextManager().enable(),
  });

  instrumentations.forEach((instrumentation) => {
    instrumentation.setTracerProvider(provider);
    instrumentation.enable();
  });

  initialized = true;
}

export function getAppTracer() {
  return tracer;
}

export function annotateSpanWithTelemetryContext(span: Span): void {
  span.setAttribute("app.environment", environment);
  span.setAttribute("app.service_name", serviceName);
  applyUserAttributes(span);
}

export function setTelemetryUserContext(user: TelemetryUserContext): void {
  const nextSignature = buildUserSignature(user);
  if (!nextSignature || nextSignature === currentUserSignature) {
    return;
  }

  currentUserContext = user;
  currentUserSignature = nextSignature;

  const span = tracer.startSpan("auth.user_context");
  annotateSpanWithTelemetryContext(span);
  span.setAttribute("auth.state", "authenticated");
  span.end();
}

export function clearTelemetryUserContext(): void {
  currentUserContext = null;
  currentUserSignature = null;
}