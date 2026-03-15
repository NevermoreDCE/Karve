import { createContext, useContext, useEffect, type ReactNode } from "react";
import { useMsal, useIsAuthenticated } from "@azure/msal-react";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { apiLoginRequest } from "./authConfig";
import { configureApiClient } from "../api/apiClient";
import {
  clearTelemetryUserContext,
  setTelemetryUserContext,
} from "../observability/otel";

interface AuthContextValue {
  isAuthenticated: boolean;
  login: () => void;
  logout: () => void;
  getAccessToken: () => Promise<string | null>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function readClaim(
  claims: Record<string, unknown> | undefined,
  name: string
): string | null {
  const value = claims?.[name];
  return typeof value === "string" ? value : null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const isE2eAuthBypass = import.meta.env.VITE_E2E_AUTH_BYPASS === "true";
  const effectiveIsAuthenticated = isE2eAuthBypass || isAuthenticated;

  const login = () => {
    if (isE2eAuthBypass) {
      return;
    }
    instance.loginRedirect(apiLoginRequest);
  };

  const logout = () => {
    if (isE2eAuthBypass) {
      return;
    }
    instance.logoutRedirect();
  };

  const getAccessToken = async (): Promise<string | null> => {
    if (isE2eAuthBypass) {
      return "e2e-access-token";
    }

    if (accounts.length === 0) return null;

    try {
      const response = await instance.acquireTokenSilent({
        ...apiLoginRequest,
        account: accounts[0],
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        instance.acquireTokenRedirect({
          ...apiLoginRequest,
          account: accounts[0],
        });
      }
      return null;
    }
  };

  // Wire up Axios interceptors whenever auth state changes so the API client
  // always has a fresh token getter and a valid unauthorized handler.
  useEffect(() => {
    const cleanup = configureApiClient(getAccessToken, login);
    return cleanup;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [effectiveIsAuthenticated]);

  useEffect(() => {
    if (isE2eAuthBypass) {
      setTelemetryUserContext({
        userId: "e2e-user-id",
        email: "e2e.user@karve.local",
        displayName: "Karve E2E User",
      });
      return;
    }

    const account = accounts[0];
    if (!isAuthenticated || !account) {
      clearTelemetryUserContext();
      return;
    }

    const claims = (account.idTokenClaims ?? {}) as Record<string, unknown>;

    setTelemetryUserContext({
      userId:
        readClaim(claims, "oid") ??
        readClaim(claims, "sub") ??
        account.homeAccountId ??
        null,
      email:
        readClaim(claims, "preferred_username") ??
        readClaim(claims, "email") ??
        account.username ??
        null,
      displayName:
        readClaim(claims, "name") ??
        (typeof account.name === "string" ? account.name : null),
    });
  }, [accounts, isAuthenticated, isE2eAuthBypass]);

  return (
    <AuthContext.Provider value={{ isAuthenticated: effectiveIsAuthenticated, login, logout, getAccessToken }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuthContext(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuthContext must be used within an AuthProvider");
  }
  return context;
}
