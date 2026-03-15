import type { ErrorInfo, ReactNode } from "react";
import { Component } from "react";
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
        <section>
          <h2>Something went wrong.</h2>
          <p>Please refresh the page or try another section.</p>
        </section>
      );
    }

    return this.props.children;
  }
}
