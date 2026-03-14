import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'react-hot-toast'
import { queryClient } from './api/queryClient'
import { AuthProvider } from './auth/AuthProvider'
import { ErrorBoundary } from './components/ErrorBoundary'
import { NavBar } from './components/NavBar'
import { ProtectedRoute } from './components/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { InvoicesPage } from './pages/InvoicesPage'
import { InvoiceDetailPage } from './pages/InvoiceDetailPage'
import { CustomersPage } from './pages/CustomersPage'
import { ProductsPage } from './pages/ProductsPage'
import './App.css'

function ProtectedPage({ children }: { children: ReactNode }) {
  return (
    <ProtectedRoute>
      <ErrorBoundary>{children}</ErrorBoundary>
    </ProtectedRoute>
  )
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Toaster position="top-right" />
      <BrowserRouter>
        <AuthProvider>
          <NavBar />
          <main>
            <Routes>
              <Route path="/login" element={<ErrorBoundary><LoginPage /></ErrorBoundary>} />
              <Route
                path="/"
                element={
                  <ProtectedPage>
                    <DashboardPage />
                  </ProtectedPage>
                }
              />
              <Route
                path="/invoices"
                element={
                  <ProtectedPage>
                    <InvoicesPage />
                  </ProtectedPage>
                }
              />
              <Route
                path="/invoices/:id"
                element={
                  <ProtectedPage>
                    <InvoiceDetailPage />
                  </ProtectedPage>
                }
              />
              <Route
                path="/customers"
                element={
                  <ProtectedPage>
                    <CustomersPage />
                  </ProtectedPage>
                }
              />
              <Route
                path="/products"
                element={
                  <ProtectedPage>
                    <ProductsPage />
                  </ProtectedPage>
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
