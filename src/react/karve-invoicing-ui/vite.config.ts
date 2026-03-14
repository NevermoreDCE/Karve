import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes('node_modules')) {
            return undefined
          }

          if (id.includes('@opentelemetry')) {
            return 'otel-vendor'
          }

          if (id.includes('@azure/msal')) {
            return 'msal-vendor'
          }

          if (id.includes('react-router-dom')) {
            return 'router-vendor'
          }

          if (id.includes('@tanstack/react-query') || id.includes('react-hook-form') || id.includes('zustand')) {
            return 'app-vendor'
          }

          if (id.includes('react') || id.includes('scheduler')) {
            return 'react-vendor'
          }

          return 'vendor'
        },
      },
    },
  },
})
