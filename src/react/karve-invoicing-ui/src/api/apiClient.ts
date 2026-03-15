import axios, {
  type AxiosInstance,
  type InternalAxiosRequestConfig,
  type AxiosResponse,
  type AxiosError,
} from "axios";
import { context, propagation, trace, SpanStatusCode, type Span } from "@opentelemetry/api";
import toast from "react-hot-toast";
import { annotateSpanWithTelemetryContext, getAppTracer } from "../observability/otel";
import { useTenantStore } from "../state/tenantStore";

function requiredEnv(name: string): string {
  const value = import.meta.env[name as keyof ImportMetaEnv];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

const BASE_URL = requiredEnv("VITE_API_BASE_URL");

/** Shared Axios instance used by all API modules. */
export const apiClient: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// ─── Interceptor handles ─────────────────────────────────────────────────────
// Stored so they can be ejected when reconfigured (e.g. on logout).

let requestInterceptorId: number | null = null;
let responseInterceptorId: number | null = null;

interface TracedRequestConfig extends InternalAxiosRequestConfig {
  otelRequestSpan?: Span;
}

/**
 * Attaches Bearer-token injection and 401/403 response handling to the shared
 * Axios instance.  Call this once from AuthProvider when the user is
 * authenticated.  Returns a cleanup function that removes the interceptors.
 *
 * @param getToken   Async function that returns a valid access token, or null.
 * @param onUnauthorized  Called when the API returns 401 (triggers re-login).
 */
export function configureApiClient(
  getToken: () => Promise<string | null>,
  onUnauthorized: () => void
): () => void {
  // Eject any previously registered interceptors.
  if (requestInterceptorId !== null) {
    apiClient.interceptors.request.eject(requestInterceptorId);
  }
  if (responseInterceptorId !== null) {
    apiClient.interceptors.response.eject(responseInterceptorId);
  }

  // D2 — Request interceptor: attach Bearer token.
  requestInterceptorId = apiClient.interceptors.request.use(
    async (config: InternalAxiosRequestConfig) => {
      const tracedConfig = config as TracedRequestConfig;
      const method = (config.method ?? "get").toUpperCase();
      const requestSpan = getAppTracer().startSpan(`http.client.${method}`);

      annotateSpanWithTelemetryContext(requestSpan);
      requestSpan.setAttribute("http.request.method", method);
      requestSpan.setAttribute("http.url", `${BASE_URL}${config.url ?? ""}`);

      const traceContext = trace.setSpan(context.active(), requestSpan);
      const traceHeaders: Record<string, string> = {};
      propagation.inject(traceContext, traceHeaders);

      const traceParent = traceHeaders.traceparent;
      if (traceParent) {
        config.headers.traceparent = traceParent;
      }

      const traceState = traceHeaders.tracestate;
      if (traceState) {
        config.headers.tracestate = traceState;
      }

      const token = await getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }

      const selectedCompanyId = useTenantStore.getState().selectedCompanyId;
      if (selectedCompanyId) {
        config.headers["X-Company-Id"] = selectedCompanyId;
      }

      tracedConfig.otelRequestSpan = requestSpan;

      return config;
    },
    (error: unknown) => Promise.reject(error)
  );

  // D3 — Response interceptor: handle 401 and 403.
  responseInterceptorId = apiClient.interceptors.response.use(
    (response: AxiosResponse) => {
      const tracedConfig = response.config as TracedRequestConfig;
      tracedConfig.otelRequestSpan?.setAttribute("http.response.status_code", response.status);
      tracedConfig.otelRequestSpan?.setStatus({ code: SpanStatusCode.OK });
      tracedConfig.otelRequestSpan?.end();
      return response;
    },
    (error: AxiosError) => {
      const tracedConfig = error.config as TracedRequestConfig | undefined;
      const statusCode = error.response?.status;
      if (statusCode) {
        tracedConfig?.otelRequestSpan?.setAttribute("http.response.status_code", statusCode);
      }
      tracedConfig?.otelRequestSpan?.recordException(error);
      tracedConfig?.otelRequestSpan?.setStatus({ code: SpanStatusCode.ERROR, message: error.message });
      tracedConfig?.otelRequestSpan?.end();

      const status = error.response?.status;
      if (status === 401) {
        onUnauthorized();
      } else if (status === 403) {
        toast.error("Access denied for this company or resource.");
        console.error("[API] Access denied (403):", error.config?.url);
      } else if (status && status >= 500) {
        toast.error("Server error. Please try again shortly.");
      } else if (status && status >= 400) {
        toast.error("Request failed. Please check the input and retry.");
      }
      return Promise.reject(error);
    }
  );

  return () => {
    if (requestInterceptorId !== null) {
      apiClient.interceptors.request.eject(requestInterceptorId);
      requestInterceptorId = null;
    }
    if (responseInterceptorId !== null) {
      apiClient.interceptors.response.eject(responseInterceptorId);
      responseInterceptorId = null;
    }
  };
}
