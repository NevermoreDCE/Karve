import { test } from "@playwright/test";

// This is intentionally scaffolded and skipped for now because create-invoice
// flow requires authenticated context and seeded test data (company/customer).
// Once available, remove .skip and automate create flow end-to-end.
test.describe("Create invoice", () => {
  test.skip("creates a new invoice from the invoices page", async () => {
    // TODO: 1) authenticate, 2) open create form, 3) submit, 4) assert new row.
  });
});
