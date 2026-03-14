import { Navigate, type ReactNode } from "react-router-dom";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";

interface ProtectedRouteProps {
  children: ReactNode;
}

/**
 * Renders children only when the user is authenticated.
 * While MSAL is processing a redirect, a loading indicator is shown.
 * Unauthenticated users are redirected to /login.
 */
export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const isAuthenticated = useIsAuthenticated();
  const { inProgress } = useMsal();

  // MSAL is still processing the redirect response — do not redirect yet.
  if (inProgress !== InteractionStatus.None) {
    return <div className="auth-loading">Authenticating…</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
