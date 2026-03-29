# 🧩 **Task Group — Convert Project Karve Front‑End to Material UI (MUI)**  
### *Modern, consistent, enterprise‑grade UI components across the entire SPA*

---

# 🎯 **Goals**
By the end of this task group, you will have:

- MUI installed and configured  
- A global theme (light + dark)  
- A reusable layout system (AppBar + Drawer + Content)  
- MUI‑based form controls, dialogs, tables, buttons, switches  
- All existing custom components migrated to MUI equivalents  
- Consistent spacing, typography, and color system  
- Zero rendering glitches from hand‑rolled components  

---

# 🧩 **Task Group A — Install and Configure MUI**

### **A1 — Install MUI core + icons**
```bash
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled
```

### **A2 — Create `/theme` folder**
Inside `src/theme`:
- `theme.ts`
- `palette.ts`
- `typography.ts`
- `components.ts` (component overrides)

### **A3 — Create base theme**
In `theme.ts`:
- Import `createTheme`
- Add:
  - Primary/secondary colors  
  - Typography scale  
  - Shape (border radius)  
  - Component defaults (Buttons, TextFields, Dialogs)

### **A4 — Add dark mode theme**
Create `darkTheme.ts` with:
- `palette.mode = 'dark'`
- Adjusted background + surface colors

### **A5 — Wrap app in `ThemeProvider`**
In `main.tsx`:
```tsx
<ThemeProvider theme={theme}>
  <CssBaseline />
  <App />
</ThemeProvider>
```

---

# 🧩 **Task Group B — Add Global Layout Using MUI**

### **B1 — Create `/layout` folder**
Files:
- `AppLayout.tsx`
- `AppHeader.tsx`
- `AppSidebar.tsx`
- `AppContent.tsx`

### **B2 — Implement `AppHeader`**
Use:
- `<AppBar />`
- `<Toolbar />`
- `<IconButton />`
- `<Typography />`

### **B3 — Implement `AppSidebar`**
Use:
- `<Drawer />`
- `<List />`
- `<ListItemButton />`
- `<ListItemIcon />`
- `<ListItemText />`

### **B4 — Implement `AppContent`**
Use:
- `<Box sx={{ p: 3 }}>`

### **B5 — Update `App.tsx` to wrap all routes in `AppLayout`**

---

# 🧩 **Task Group C — Replace All Form Controls With MUI**

### **C1 — Replace `<input>` with `<TextField>`**
Search for:
- `<input>`
- `<textarea>`
- `<select>`

Replace with:
- `<TextField />`
- `<Select />`
- `<MenuItem />`

### **C2 — Replace custom switches with `<Switch />`**

### **C3 — Replace custom checkboxes with `<Checkbox />`**

### **C4 — Replace custom radio buttons with `<Radio />` + `<RadioGroup />`**

### **C5 — Replace custom date pickers with MUI X Date Pickers**
```bash
npm install @mui/x-date-pickers
```

---

# 🧩 **Task Group D — Replace All Buttons With MUI Buttons**

### **D1 — Replace `<button>` with `<Button>`**
Use variants:
- `contained`
- `outlined`
- `text`

### **D2 — Standardize button sizes + spacing**
Use theme defaults.

---

# 🧩 **Task Group E — Replace All Dialogs With MUI Dialogs**

### **E1 — Create `ConfirmDialog.tsx` reusable component**
Use:
- `<Dialog />`
- `<DialogTitle />`
- `<DialogContent />`
- `<DialogActions />`

### **E2 — Replace all custom modals with `ConfirmDialog` or `Dialog`**

### **E3 — Add transitions**
Use:
- `Slide` or `Fade`

---

# 🧩 **Task Group F — Replace All Tables With MUI Tables**

### **F1 — Create `DataTable.tsx` wrapper**
Use:
- `<Table />`
- `<TableHead />`
- `<TableRow />`
- `<TableCell />`
- `<TableBody />`

### **F2 — Add sorting + pagination**
Use:
- `<TableSortLabel />`
- `<TablePagination />`

### **F3 — Replace all invoice/customer/product tables with `DataTable`**

---

# 🧩 **Task Group G — Replace All Cards With MUI Cards**

### **G1 — Create `DashboardCard.tsx`**
Use:
- `<Card />`
- `<CardContent />`
- `<Typography />`

### **G2 — Replace all dashboard widgets to use `DashboardCard`**

---

# 🧩 **Task Group H — Replace All Alerts, Snackbars, and Toasts**

### **H1 — Install MUI Snackbar provider**
Use:
- `<Snackbar />`
- `<Alert />`

### **H2 — Create `useSnackbar()` hook**
Wrap MUI Snackbar logic.

### **H3 — Replace react-hot-toast with MUI Snackbar**

---

# 🧩 **Task Group I — Replace All Icons With MUI Icons**

### **I1 — Replace custom SVGs with MUI icons**
Examples:
- `<Add />`
- `<Edit />`
- `<Delete />`
- `<Menu />`
- `<Dashboard />`

### **I2 — Standardize icon sizes via theme**

---

# 🧩 **Task Group J — Update All Pages to Use MUI Layout + Components**

### **J1 — Dashboard Page**
- Replace grid with `<Grid container spacing={2}>`
- Wrap widgets in `<Grid item xs={12} md={6} lg={4}>`

### **J2 — Invoice Pages**
- Replace forms with MUI form controls
- Replace tables with MUI tables
- Replace dialogs with MUI dialogs

### **J3 — Customer Pages**
Same as invoices.

### **J4 — Product Pages**
Same as invoices.

### **J5 — Admin Pages (Companies + Users)**
- Use MUI DataGrid or Table
- Use MUI Dialog for create/edit

---

# 🧩 **Task Group K — Add MUI Theming for Light/Dark Mode**

### **K1 — Add Zustand store for theme mode**
- `themeMode: 'light' | 'dark'`
- `toggleTheme()`

### **K2 — Update `ThemeProvider` to switch themes dynamically**

### **K3 — Add theme toggle button in `AppHeader`**
Use:
- `<IconButton>`
- `<LightMode />` / `<DarkMode />`

---

# 🧩 **Task Group L — Add MUI‑Friendly Global Styles**

### **L1 — Remove custom CSS that conflicts with MUI**

### **L2 — Add `CssBaseline`**
Already done in A5.

### **L3 — Add global spacing + typography rules in theme**

---

# 🧩 **Task Group M — Observability + Resilience Integration**

### **M1 — Wrap all MUI dialogs + forms in OTel spans**

### **M2 — Add error boundaries around MUI components**

### **M3 — Ensure Axios interceptors show MUI Snackbars on errors**

---

# 🎉 **End Result**

After completing this task group, your React front‑end will:

- Look and feel like a polished enterprise SaaS product  
- Have consistent UI components across all pages  
- Use a modern design system with light/dark mode  
- Eliminate rendering glitches from hand‑rolled components  
- Be easier to maintain and extend  
- Match the quality of a MudBlazor‑based UI, but in React  
