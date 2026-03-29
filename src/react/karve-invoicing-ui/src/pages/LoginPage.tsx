import { Navigate } from "react-router-dom";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";
import { Box, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material";
import { useAuth } from "../hooks/useAuth";

export function LoginPage() {
  const { login } = useAuth();
  const isAuthenticated = useIsAuthenticated();
  const { inProgress } = useMsal();
  const isE2eAuthBypass = import.meta.env.VITE_E2E_AUTH_BYPASS === "true";

  // If MSAL is still processing a redirect, wait before deciding
  if (inProgress !== InteractionStatus.None && !isE2eAuthBypass) {
    return (
      <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center", p: 2 }}>
        <Stack direction="row" spacing={1.25} alignItems="center">
          <CircularProgress size={20} />
          <Typography variant="body2" color="text.secondary">Authenticating...</Typography>
        </Stack>
      </Box>
    );
  }

  // Already logged in — redirect to the root
  if (isAuthenticated || isE2eAuthBypass) {
    return <Navigate to="/" replace />;
  }

  return (
    <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center", p: 2 }}>
      <Paper variant="outlined" sx={{ p: { xs: 3, sm: 4 }, width: "100%", maxWidth: 460 }}>
        <Stack spacing={2.25} alignItems="center" textAlign="center">
          <Typography variant="h4" component="h1">Karve Invoicing</Typography>
          <Typography variant="body2" color="text.secondary">
            Sign in with your Microsoft account to continue.
          </Typography>
          <Button variant="contained" size="large" onClick={login} sx={{ minWidth: 190 }}>
            Sign in
          </Button>
        </Stack>
      </Paper>
    </Box>
  );
}

