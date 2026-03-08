
# 🌐 **STEP 4 — React Frontend + OAuth PKCE + API Integration**

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

### **A1 — Create the React + TypeScript project using Vite**
From the `/src` folder:

```bash
npm create vite@latest karve-invoicing-ui -- --template react-ts
```

### **A2 — Add required dependencies**
```bash
cd karve-invoicing-ui
npm install @azure/msal-browser @azure/msal-react
npm install axios
npm install @tanstack/react-query
npm install react-router-dom
npm install zustand
```

### **A3 — Add dev dependencies**
```bash
npm install -D eslint prettier @types/node
```

### **A4 — Create folder structure**
Inside `src/`:

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

### **B1 — Create `authConfig.ts`**
Include:
- clientId  
- authority  
- redirectUri  
- cache settings  

### **B2 — Initialize MSAL in `main.tsx`**
Wrap `<App />` with:

```tsx
<MsalProvider instance={msalInstance}>
```

### **B3 — Create `AuthProvider.tsx`**
Responsibilities:
- Handle login redirect  
- Handle logout  
- Expose authentication state  

### **B4 — Create `useAuth()` hook**
Returns:
- `isAuthenticated`
- `login()`
- `logout()`
- `getAccessToken()`

### **B5 — Add login and logout buttons**
In a top‑level navigation component.

---

# 🧩 Task Group C — Implement Protected Routes

### **C1 — Create `ProtectedRoute.tsx`**
If not authenticated → redirect to login.

### **C2 — Update `App.tsx` routing**
Example:

```tsx
<Route path="/invoices" element={<ProtectedRoute><InvoicesPage /></ProtectedRoute>} />
```

---

# 🧩 Task Group D — Create API Client With Token Injection

### **D1 — Create `apiClient.ts`**
Use Axios or Fetch.

### **D2 — Add request interceptor**
Attach Bearer token:

```ts
config.headers.Authorization = `Bearer ${token}`;
```

### **D3 — Add response interceptor**
Handle:
- 401 → trigger login  
- 403 → show “Access denied”  

### **D4 — Add typed API methods**
In `/api` folder:
- `getInvoices()`
- `getInvoice(id)`
- `createInvoice()`
- `updateInvoice()`
- `deleteInvoice()`
- Same for customers, products, payments

---

# 🧩 Task Group E — Add React Query for Data Fetching

### **E1 — Create `queryClient.ts`**
Initialize React Query client.

### **E2 — Wrap app in `<QueryClientProvider>`**

### **E3 — Create hooks**
In `/hooks`:

- `useInvoices()`
- `useInvoice(id)`
- `useCreateInvoice()`
- `useUpdateInvoice()`
- `useDeleteInvoice()`

### **E4 — Add optimistic updates for mutations**

---

# 🧩 Task Group F — Build Basic UI Pages

### **F1 — Create `InvoicesPage.tsx`**
- Table of invoices  
- “Create Invoice” button  
- Link to invoice detail  

### **F2 — Create `InvoiceDetailPage.tsx`**
- Show invoice fields  
- List line items  
- List payments  
- Buttons for edit/delete  

### **F3 — Create `CustomersPage.tsx`**
- Table of customers  
- Create/edit forms  

### **F4 — Create `ProductsPage.tsx`**
- Table of products  
- Create/edit forms  

### **F5 — Create `DashboardPage.tsx`**
- Placeholder for now  

---

# 🧩 Task Group G — Add Forms With Validation

### **G1 — Install React Hook Form**
```bash
npm install react-hook-form
```

### **G2 — Create form components**
- `InvoiceForm.tsx`
- `CustomerForm.tsx`
- `ProductForm.tsx`

### **G3 — Add validation rules**
Match your backend validators.

---

# 🧩 Task Group H — Environment Configuration

### **H1 — Create `.env` files**
- `.env.development`
- `.env.production`

### **H2 — Add variables**
- `VITE_API_BASE_URL`
- `VITE_AZURE_AD_CLIENT_ID`
- `VITE_AZURE_AD_TENANT_ID`
- `VITE_AZURE_AD_REDIRECT_URI`

### **H3 — Update `authConfig.ts` and `apiClient.ts` to use env vars**

---

# 🧩 Task Group I — Handle Multi‑Tenant Behavior in the UI

### **I1 — Create `useCurrentUser()` hook**
Fetch:
- User profile  
- Company memberships  

### **I2 — If user has multiple companies**
Show a company selector.

### **I3 — Store selected company in Zustand state**

### **I4 — Add selected company ID to API requests**
Include header:

```
X-Company-Id: <id>
```

(Your API can use this later if needed.)

---

# 🧩 Task Group J — Add Error Handling + Loading States

### **J1 — Create `ErrorBoundary.tsx`**

### **J2 — Create `LoadingSpinner.tsx`**

### **J3 — Wrap pages with error boundaries**

### **J4 — Add toast notifications**
Install:

```bash
npm install react-hot-toast
```

---

# 🧩 Task Group K — Add Basic Styling

### **K1 — Install Tailwind CSS**
```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### **K2 — Configure Tailwind**

### **K3 — Add layout components**
- `Navbar`
- `Sidebar`
- `PageContainer`

---

# 🧩 Task Group L — Add E2E Test Scaffolding (Optional for Step 4)

### **L1 — Install Playwright**
```bash
npx playwright install
```

### **L2 — Add basic tests**
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

