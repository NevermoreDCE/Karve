import { test } from "@playwright/test";

// This is intentionally scaffolded and skipped for now because the live app
// requires Azure AD auth before invoices can be fetched.
// Once a test tenant + auth bootstrap are available, remove .skip and add
// authenticated setup (storage state or programmatic token seeding).
test.describe("Fetch invoices", () => {
  test.skip("lists invoices for an authenticated user", async () => {
    // TODO: 1) authenticate, 2) navigate to /invoices, 3) assert invoice rows.
  });
});
