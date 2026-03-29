import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";
import { Box, CircularProgress, Typography } from "@mui/material";

interface ProtectedRouteProps {
  children: ReactNode;
}

/**
 * Renders children only when the user is authenticated.
 * While MSAL is processing a redirect, a loading indicator is shown.
 * Unauthenticated users are redirected to /login.
 */
export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const isE2eAuthBypass = import.meta.env.VITE_E2E_AUTH_BYPASS === "true";
  const isAuthenticated = useIsAuthenticated();
  const { inProgress } = useMsal();

  if (isE2eAuthBypass) {
    return <>{children}</>;
  }

  // MSAL is still processing the redirect response — do not redirect yet.
  if (inProgress !== InteractionStatus.None) {
    return (
      <Box sx={{ minHeight: "60vh", display: "grid", placeItems: "center" }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1.25 }}>
          <CircularProgress size={20} />
          <Typography variant="body2" color="text.secondary">Authenticating...</Typography>
        </Box>
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
