import { create } from "zustand";

type ThemeMode = "light" | "dark";

const COOKIE_NAME = "karve-theme";
const COOKIE_MAX_AGE = 365 * 24 * 60 * 60; // 1 year

function readCookie(): ThemeMode | null {
  const match = document.cookie
    .split(";")
    .map((c) => c.trim())
    .find((c) => c.startsWith(`${COOKIE_NAME}=`));
  const value = match?.split("=")[1];
  return value === "dark" || value === "light" ? value : null;
}

function writeCookie(theme: ThemeMode): void {
  document.cookie = `${COOKIE_NAME}=${theme}; max-age=${COOKIE_MAX_AGE}; path=/; SameSite=Lax`;
}

function detectSystemTheme(): ThemeMode {
  return window.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light";
}

function applyTheme(theme: ThemeMode): void {
  document.documentElement.setAttribute("data-theme", theme);
}

function resolveInitialTheme(): ThemeMode {
  const saved = readCookie();
  if (saved) return saved;
  const system = detectSystemTheme();
  writeCookie(system);
  return system;
}

interface ThemeState {
  themeMode: ThemeMode;
  theme: ThemeMode;
  toggleTheme: () => void;
  setTheme: (theme: ThemeMode) => void;
}

const initialTheme = resolveInitialTheme();
applyTheme(initialTheme);

export const useThemeStore = create<ThemeState>((set) => ({
  themeMode: initialTheme,
  theme: initialTheme,
  toggleTheme: () =>
    set((state) => {
      const next: ThemeMode = state.themeMode === "light" ? "dark" : "light";
      writeCookie(next);
      applyTheme(next);
      return { themeMode: next, theme: next };
    }),
  setTheme: (theme: ThemeMode) => {
    writeCookie(theme);
    applyTheme(theme);
    set({ themeMode: theme, theme });
  },
}));
