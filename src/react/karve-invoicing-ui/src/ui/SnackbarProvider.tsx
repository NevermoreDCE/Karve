import type { ReactNode } from "react";
import { SnackbarProvider as NotistackProvider } from "notistack";

interface SnackbarProviderProps {
  children: ReactNode;
}

export function SnackbarProvider({ children }: SnackbarProviderProps) {
  return (
    <NotistackProvider
      maxSnack={3}
      autoHideDuration={3500}
      anchorOrigin={{ vertical: "top", horizontal: "right" }}
      preventDuplicate
    >
      {children}
    </NotistackProvider>
  );
}
