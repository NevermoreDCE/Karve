import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from './api/queryClient'
import { AuthProvider } from './auth/AuthProvider'
import { NavBar } from './components/NavBar'
import { ProtectedRoute } from './components/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { InvoicesPage } from './pages/InvoicesPage'
import { InvoiceDetailPage } from './pages/InvoiceDetailPage'
import { CustomersPage } from './pages/CustomersPage'
import { ProductsPage } from './pages/ProductsPage'
import './App.css'

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <NavBar />
          <main>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <DashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/invoices"
                element={
                  <ProtectedRoute>
                    <InvoicesPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/invoices/:id"
                element={
                  <ProtectedRoute>
                    <InvoiceDetailPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/customers"
                element={
                  <ProtectedRoute>
                    <CustomersPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/products"
                element={
                  <ProtectedRoute>
                    <ProductsPage />
                  </ProtectedRoute>
                }
              />
              {/* Catch-all redirect */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </main>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

export default App
