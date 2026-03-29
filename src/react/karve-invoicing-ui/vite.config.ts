import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'node:fs';
import path from 'node:path';
import { execSync } from 'node:child_process';

// Helper to export and load the .NET dev cert for Vite dev server
const tryExportDotnetDevCert = () => {
  try {
    const certDir = path.resolve(process.cwd(), '.certs');
    const exportPath = path.join(certDir, 'aspnet-devcert.pfx');
    const passphrase = 'vite-devcert';

    if (!fs.existsSync(certDir)) {
      fs.mkdirSync(certDir, { recursive: true });
    }

    // Ensure a valid/trusted dev cert exists. If check fails (expired/missing), recreate it.
    try {
      execSync('dotnet dev-certs https --check', { stdio: 'ignore' });
    } catch {
      try { execSync('dotnet dev-certs https --clean', { stdio: 'ignore' }); } catch {}
      execSync('dotnet dev-certs https --trust', { stdio: 'ignore' });
    }

    execSync(`dotnet dev-certs https --export-path "${exportPath}" --password "${passphrase}"`, { stdio: 'ignore' });

    if (fs.existsSync(exportPath)) {
      return {
        pfx: fs.readFileSync(exportPath),
        passphrase,
      };
    }
  } catch {
    // Could not configure dotnet development certificate.
  }
  return undefined;
};

// https://vite.dev/config/
export default defineConfig(({ command }) => {
  const httpsConfig = command === 'serve' ? tryExportDotnetDevCert() : undefined;
  return {
    plugins: [react()],
    server: {
      https: httpsConfig,
      host: 'localhost',
      port: 5173,
      strictPort: true,
    },
    build: {
      rollupOptions: {
        output: {
          manualChunks(id) {
            if (!id.includes('node_modules')) {
              return undefined;
            }
            if (id.includes('@opentelemetry')) {
              return 'otel-vendor';
            }
            if (id.includes('@azure/msal')) {
              return 'msal-vendor';
            }
            if (id.includes('react-router-dom')) {
              return 'router-vendor';
            }
            if (id.includes('@tanstack/react-query') || id.includes('react-hook-form') || id.includes('zustand')) {
              return 'app-vendor';
            }
            if (id.includes('react') || id.includes('scheduler')) {
              return 'react-vendor';
            }
            return 'vendor';
          },
        },
      },
    },
  };
});
