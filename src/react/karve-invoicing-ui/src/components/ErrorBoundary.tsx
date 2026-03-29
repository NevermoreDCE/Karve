import type { ErrorInfo, ReactNode } from "react";
import { Component } from "react";
import { Alert, AlertTitle, Box, Button } from "@mui/material";
import { reportUiError } from "../observability/otel";

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  public state: ErrorBoundaryState = { hasError: false };

  public static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error("[ErrorBoundary] UI render error", error, errorInfo);
    reportUiError(error, {
      "error.source": "react.error_boundary",
      "error.component_stack": errorInfo.componentStack,
    });
  }

  public render(): ReactNode {
    if (this.state.hasError) {
      return (
        <Box sx={{ p: 3 }}>
          <Alert
            severity="error"
            action={
              <Button color="inherit" size="small" onClick={() => window.location.reload()}>
                Refresh
              </Button>
            }
          >
            <AlertTitle>Something went wrong</AlertTitle>
            Please refresh the page or try another section.
          </Alert>
        </Box>
      );
    }

    return this.props.children;
  }
}
