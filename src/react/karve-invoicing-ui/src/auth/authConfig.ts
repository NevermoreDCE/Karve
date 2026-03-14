import { type Configuration, LogLevel } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_AD_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_AD_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_AZURE_AD_REDIRECT_URI,
    postLogoutRedirectUri: import.meta.env.VITE_AZURE_AD_REDIRECT_URI,
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
  scopes: [import.meta.env.VITE_AZURE_AD_API_SCOPE],
};
