import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'react-hot-toast'
import { queryClient } from './api/queryClient'
import { AuthProvider } from './auth/AuthProvider'
import { ErrorBoundary } from './components/ErrorBoundary'
import { Navbar } from './components/NavBar'
import { Sidebar } from './components/Sidebar'
import { PageContainer } from './components/PageContainer'
import { ProtectedRoute } from './components/ProtectedRoute'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { InvoicesPage } from './pages/InvoicesPage'
import { InvoiceDetailPage } from './pages/InvoiceDetailPage'
import { CustomersPage } from './pages/CustomersPage'
import { ProductsPage } from './pages/ProductsPage'

function ProtectedPage({ children }: { children: ReactNode }) {
  return (
    <ProtectedRoute>
      <PageContainer sidebar={<Sidebar />}>{children}</PageContainer>
    </ProtectedRoute>
  )
}

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <Toaster position="top-right" />
        <BrowserRouter>
          <AuthProvider>
            <div className="min-h-screen">
              <Navbar />
              <main>
              <Routes>
                <Route
                  path="/login"
                  element={
                    <PageContainer>
                      <LoginPage />
                    </PageContainer>
                  }
                />
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
            </div>
          </AuthProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </ErrorBoundary>
  )
}

export default App
