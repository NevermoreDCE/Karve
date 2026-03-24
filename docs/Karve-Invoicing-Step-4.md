
# 🌐 **STEP 4 — React Frontend + OAuth PKCE + API Integration**

> Verification status updated on 2026-03-22.

## 🎯 Step 4 Goals
By the end of Step 4, you will have:

- A React + TypeScript SPA created with Vite  
- Azure AD OAuth PKCE authentication  
- A secure token acquisition flow  
- An API client that attaches Bearer tokens  
- A reusable Axios (or Fetch) wrapper  
- React Query for data fetching  
- Protected routes  
- A basic UI for invoices, customers, products  
- A working login/logout flow  
- Environment‑based configuration  
- Integration with your API’s multi‑tenant enforcement  

This is the step where your system becomes a full-stack application.

---

# 🧩 Task Group A — Create the React Project

### **A1 — Create the React + TypeScript project using Vite** [DONE]
From the `/src/react` folder:

```bash
npm create vite@latest karve-invoicing-ui -- --template react-ts
```

### **A2 — Add required dependencies** [DONE]
```bash
cd karve-invoicing-ui
npm install @azure/msal-browser @azure/msal-react
npm install axios
npm install @tanstack/react-query
npm install react-router-dom
npm install zustand
```

### **A3 — Add dev dependencies** [DONE]
```bash
npm install -D eslint prettier @types/node
```

### **A4 — Create folder structure** [DONE]
Inside `src/react/`:

```
/auth
/api
/components
/hooks
/pages
/state
/types
/utils
```

---

# 🧩 Task Group B — Configure OAuth PKCE with Azure AD

### **B1 — Create `authConfig.ts`** [DONE]
Include:
- clientId  
- authority  
- redirectUri  
- cache settings  

### **B2 — Initialize MSAL in `main.tsx`** [DONE]
Wrap `<App />` with:

```tsx
<MsalProvider instance={msalInstance}>
```

### **B3 — Create `AuthProvider.tsx`** [DONE]
Responsibilities:
- Handle login redirect  
- Handle logout  
- Expose authentication state  

### **B4 — Create `useAuth()` hook** [DONE]
Returns:
- `isAuthenticated`
- `login()`
- `logout()`
- `getAccessToken()`

### **B5 — Add login and logout buttons** [DONE]
In a top‑level navigation component.

---

# 🧩 Task Group C — Implement Protected Routes

### **C1 — Create `ProtectedRoute.tsx`** [DONE]
If not authenticated → redirect to login.

### **C2 — Update `App.tsx` routing** [DONE]
Example:

```tsx
<Route path="/invoices" element={<ProtectedRoute><InvoicesPage /></ProtectedRoute>} />
```

---

# 🧩 Task Group D — Create API Client With Token Injection

### **D1 — Create `apiClient.ts`** [DONE]
Use Axios or Fetch.

### **D2 — Add request interceptor** [DONE]
Attach Bearer token:

```ts
config.headers.Authorization = `Bearer ${token}`;
```

### **D3 — Add response interceptor** [DONE]
Handle:
- 401 → trigger login  
- 403 → show “Access denied”  

### **D4 — Add typed API methods** [DONE]
In `/api` folder:
- `getInvoices()`
- `getInvoice(id)`
- `createInvoice()`
- `updateInvoice()`
- `deleteInvoice()`
- Same for customers, products, payments

---

# 🧩 Task Group E — Add React Query for Data Fetching

### **E1 — Create `queryClient.ts`** [DONE]
Initialize React Query client.

### **E2 — Wrap app in `<QueryClientProvider>`** [DONE]

### **E3 — Create hooks** [DONE]
In `/hooks`:

- `useInvoices()`
- `useInvoice(id)`
- `useCreateInvoice()`
- `useUpdateInvoice()`
- `useDeleteInvoice()`

### **E4 — Add optimistic updates for mutations** [DONE]

---

# 🧩 Task Group F — Build Basic UI Pages

### **F1 — Create `InvoicesPage.tsx`** [DONE]
- Table of invoices  
- “Create Invoice” button  
- Link to invoice detail  

### **F2 — Create `InvoiceDetailPage.tsx`** [DONE]
- Show invoice fields  
- List line items  
- List payments  
- Buttons for edit/delete  

### **F3 — Create `CustomersPage.tsx`** [DONE]
- Table of customers  
- Create/edit forms  

### **F4 — Create `ProductsPage.tsx`** [DONE]
- Table of products  
- Create/edit forms  

### **F5 — Create `DashboardPage.tsx`** [DONE]
- Placeholder for now  

---

# 🧩 Task Group G — Add Forms With Validation

### **G1 — Install React Hook Form** [DONE]
```bash
npm install react-hook-form
```

### **G2 — Create form components** [DONE]
- `InvoiceForm.tsx`
- `CustomerForm.tsx`
- `ProductForm.tsx`

### **G3 — Add validation rules** [DONE]
Match your backend validators.

---

# 🧩 Task Group H — Environment Configuration

### **H1 — Create `.env` files** [DONE]
- `.env.development`
- `.env.production`

### **H2 — Add variables** [DONE]
- `VITE_API_BASE_URL`
- `VITE_AZURE_AD_CLIENT_ID`
- `VITE_AZURE_AD_TENANT_ID`
- `VITE_AZURE_AD_REDIRECT_URI`

### **H3 — Update `authConfig.ts` and `apiClient.ts` to use env vars** [DONE]

---

# 🧩 Task Group I — Handle Multi‑Tenant Behavior in the UI

### **I1 — Create `useCurrentUser()` hook** [DONE]
Fetch:
- User profile  
- Company memberships  

### **I2 — If user has multiple companies** [DONE]
Show a company selector.

### **I3 — Store selected company in Zustand state** [DONE]

### **I4 — Add selected company ID to API requests** [DONE]
Include header:

```
X-Company-Id: <id>
```

(Your API can use this later if needed.)

---

# 🧩 Task Group J — Add Error Handling + Loading States

### **J1 — Create `ErrorBoundary.tsx`** [DONE]

### **J2 — Create `LoadingSpinner.tsx`** [DONE]

### **J3 — Wrap pages with error boundaries** [DONE]

### **J4 — Add toast notifications** [DONE]
Install:

```bash
npm install react-hot-toast
```

---

# 🧩 Task Group K — Add Basic Styling

### **K1 — Install Tailwind CSS** [DONE]
```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### **K2 — Configure Tailwind** [DONE]

### **K3 — Add layout components** [DONE]
- `Navbar`
- `Sidebar`
- `PageContainer`

---

# 🧩 Task Group L — Add E2E Test Scaffolding (Optional for Step 4)

### **L1 — Install Playwright** [DONE]
```bash
npx playwright install
```

### **L2 — Add basic tests** [DONE]
- Login flow  
- Fetch invoices  
- Create invoice  

---

# 🎉 Step 4 Complete  
Once you finish these tasks, you will have:

- A fully authenticated React SPA  
- A secure PKCE login flow  
- A typed API client with token injection  
- React Query data fetching  
- Protected routes  
- Multi‑tenant‑aware UI  
- Working invoice/customer/product pages  
- A modern, clean, scalable frontend foundation  

