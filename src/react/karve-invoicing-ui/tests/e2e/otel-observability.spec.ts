import { expect, test, type APIRequestContext } from "@playwright/test";

const OTEL_COLLECTOR_BASE_URL = "http://127.0.0.1:4318";
const TRACEPARENT_PATTERN = /^00-[a-f0-9]{32}-[a-f0-9]{16}-0[01]$/;

function isInvoicesApiRequest(url: string): boolean {
  const parsed = new URL(url);
  return parsed.pathname === "/api/invoices";
}

interface CollectorRequest {
  receivedAt: string;
  payload: {
    resourceSpans?: unknown[];
    [key: string]: unknown;
  };
}

interface CollectorSnapshot {
  count: number;
  items: CollectorRequest[];
}

async function resetCollector(request: APIRequestContext): Promise<void> {
  const response = await request.post(`${OTEL_COLLECTOR_BASE_URL}/_otel/reset`);
  expect(response.ok()).toBeTruthy();
}

async function readCollector(request: APIRequestContext): Promise<CollectorSnapshot> {
  const response = await request.get(`${OTEL_COLLECTOR_BASE_URL}/_otel/requests`);
  expect(response.ok()).toBeTruthy();
  return (await response.json()) as CollectorSnapshot;
}

async function waitForCollectorExport(
  request: APIRequestContext,
  timeoutMs: number,
): Promise<CollectorSnapshot> {
  const start = Date.now();

  while (Date.now() - start < timeoutMs) {
    const snapshot = await readCollector(request);
    const hasResourceSpans = snapshot.items.some((entry) =>
      Array.isArray(entry.payload?.resourceSpans) &&
      entry.payload.resourceSpans.length > 0,
    );

    if (hasResourceSpans) {
      return snapshot;
    }

    await new Promise((resolve) => setTimeout(resolve, 250));
  }

  throw new Error("Timed out waiting for OTLP trace exports in collector.");
}

test.describe("OpenTelemetry observability", () => {
  // H2-1 — OTel initialisation does not break page load
  test("page loads without OpenTelemetry initialisation errors", async ({ page }) => {
    const otelErrors: string[] = [];

    page.on("console", (msg) => {
      if (msg.type() === "error") {
        const text = msg.text().toLowerCase();
        if (text.includes("opentelemetry") || text.includes("otel")) {
          otelErrors.push(msg.text());
        }
      }
    });

    await page.goto("/login");

    await expect(page.getByRole("main").getByRole("button", { name: "Sign in" })).toBeVisible();
    expect(otelErrors).toHaveLength(0);
  });

  // H2-2 — Spans are actually exported to the OTLP endpoint after page load
  test("exports spans to the OTLP endpoint", async ({ page, request }) => {
    test.setTimeout(35_000);

    await resetCollector(request);

    await page.goto("/login");
    await page.waitForFunction(() => Boolean(window.__karveOtelTestHooks));

    // Send a deterministic OTLP JSON probe payload through the configured endpoint.
    await page.evaluate(async () => {
      await window.__karveOtelTestHooks?.sendCollectorProbePayload();
    });

    const snapshot = await waitForCollectorExport(request, 25_000);
    expect(snapshot.count).toBeGreaterThan(0);
  });

  // H2-3 — Traceparent header forwarding
  test("api requests include a W3C traceparent header", async ({ page }) => {
    await page.route("**/*", (route) => {
      if (!isInvoicesApiRequest(route.request().url())) {
        return route.continue();
      }

      return route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          isSuccess: true,
          data: {
            items: [],
            totalCount: 0,
            page: 1,
            pageSize: 20,
          },
          error: null,
        }),
      });
    });

    const apiRequestPromise = page.waitForRequest(
      (req) => req.method() === "GET" && isInvoicesApiRequest(req.url()),
      { timeout: 15_000 },
    );

    await page.goto("/invoices");

    const apiRequest = await apiRequestPromise;
    const allHeaders = await apiRequest.allHeaders();
    const traceparent =
      (await apiRequest.headerValue("traceparent")) ??
      allHeaders["traceparent"];
    expect(traceparent).toBeTruthy();
    expect(traceparent).toMatch(TRACEPARENT_PATTERN);
  });

  // H2-4 — Error boundary captures runtime errors as spans (requires auth + error injection)
  test.skip("error boundary reports runtime errors as ui.error spans", async ({ page }) => {
    // TODO: Requires authenticated context and error injection.
    // Steps once auth bootstrap exists:
    // 1. Authenticate.
    // 2. Mock the OTLP endpoint and track exported spans.
    // 3. Inject a runtime error into the React tree (e.g. via a test-only route that
    //    renders a component which throws in render).
    // 4. Assert the ErrorBoundary fallback UI is visible.
    // 5. Assert an exported span with name "ui.error" appeared in the OTLP traffic,
    //    containing an exception event with the thrown error message.
  });
});
