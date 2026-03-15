import { expect, test } from "@playwright/test";

test.describe("Login flow", () => {
  test("redirects unauthenticated invoice route to login page", async ({ page }) => {
    await page.goto("/invoices");
    const main = page.getByRole("main");

    await expect(page).toHaveURL(/\/login$/);
    await expect(main.getByRole("heading", { name: "Karve Invoicing" })).toBeVisible();
    await expect(main.getByRole("button", { name: "Sign in" })).toBeVisible();
  });

  test("login page renders primary sign-in action", async ({ page }) => {
    await page.goto("/login");
    const main = page.getByRole("main");

    await expect(main.getByRole("button", { name: "Sign in" })).toBeVisible();
  });
});
