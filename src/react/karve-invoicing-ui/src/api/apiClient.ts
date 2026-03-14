import axios, {
  type AxiosInstance,
  type InternalAxiosRequestConfig,
  type AxiosResponse,
  type AxiosError,
} from "axios";

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

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
      const token = await getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error: unknown) => Promise.reject(error)
  );

  // D3 — Response interceptor: handle 401 and 403.
  responseInterceptorId = apiClient.interceptors.response.use(
    (response: AxiosResponse) => response,
    (error: AxiosError) => {
      const status = error.response?.status;
      if (status === 401) {
        onUnauthorized();
      } else if (status === 403) {
        console.error("[API] Access denied (403):", error.config?.url);
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
