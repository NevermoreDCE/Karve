import { CircularProgress, Stack, Typography } from "@mui/material";

interface LoadingSpinnerProps {
  label?: string;
}

export function LoadingSpinner({ label = "Loading..." }: LoadingSpinnerProps) {
  return (
    <Stack role="status" aria-live="polite" direction="row" spacing={1.25} alignItems="center">
      <CircularProgress size={18} />
      <Typography variant="body2" color="text.secondary">{label}</Typography>
    </Stack>
  );
}
