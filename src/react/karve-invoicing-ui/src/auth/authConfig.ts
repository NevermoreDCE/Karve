import { type Configuration, LogLevel } from "@azure/msal-browser";

function requiredEnv(name: string): string {
  const value = import.meta.env[name as keyof ImportMetaEnv];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

const azureAdClientId = requiredEnv("VITE_AZURE_AD_CLIENT_ID");
const azureAdTenantId = requiredEnv("VITE_AZURE_AD_TENANT_ID");
const azureAdRedirectUri = requiredEnv("VITE_AZURE_AD_REDIRECT_URI");
const azureAdApiScope = requiredEnv("VITE_AZURE_AD_API_SCOPE");

export const msalConfig: Configuration = {
  auth: {
    clientId: azureAdClientId,
    authority: `https://login.microsoftonline.com/${azureAdTenantId}`,
    redirectUri: azureAdRedirectUri,
    postLogoutRedirectUri: azureAdRedirectUri,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        switch (level) {
          case LogLevel.Error:
            console.error("[MSAL]", message);
            break;
          case LogLevel.Warning:
            console.warn("[MSAL]", message);
            break;
        }
      },
    },
  },
};

/** Scopes requested when acquiring a token for the Karve Invoicing API. */
export const apiLoginRequest = {
  scopes: [azureAdApiScope],
};
