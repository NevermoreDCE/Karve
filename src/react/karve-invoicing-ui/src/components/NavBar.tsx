import { useState } from "react";
import { useAuth } from "../hooks/useAuth";
import { useCurrentUser } from "../hooks/useCurrentUser";
import { useThemeStore } from "../state/themeStore";
import { Modal } from "./Modal";

export function Navbar() {
  const { isAuthenticated, login, logout } = useAuth();
  const {
    profile,
    memberships,
    selectedCompanyId,
    setSelectedCompanyId,
    isLoading,
  } = useCurrentUser();
  const { theme, toggleTheme } = useThemeStore();
  const [showPreferences, setShowPreferences] = useState(false);

  // Prefer displayName (name claim = "Given Family") over email
  const displayName = profile.displayName ?? profile.email;

  return (
    <>
      <header className="sticky top-0 z-20 border-b border-[color:var(--line-color)] bg-[color:var(--panel-color)]/95 backdrop-blur">
        <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6 lg:px-8">
          <div>
            <h1 className="font-heading text-lg tracking-tight" style={{ color: "var(--text-color)" }}>
              Karve Invoicing
            </h1>
            {isAuthenticated && displayName ? (
              <p className="text-xs" style={{ color: "var(--muted-color)" }}>{displayName}</p>
            ) : null}
          </div>

          <div className="flex items-center gap-3">
            {isAuthenticated && memberships.length > 1 ? (
              <label className="flex items-center gap-2 text-sm" style={{ color: "var(--text-color)" }}>
                <span>Company</span>
                <select
                  className="rounded-md border px-2 py-1 text-sm"
                  style={{ borderColor: "var(--line-color)", background: "var(--panel-color)", color: "var(--text-color)" }}
                  value={selectedCompanyId ?? ""}
                  onChange={(e) => setSelectedCompanyId(e.target.value)}
                  disabled={isLoading}
                >
                  {memberships.map((company) => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
              </label>
            ) : null}

            {/* Settings / User Preferences icon */}
            {isAuthenticated ? (
              <button
                type="button"
                className="btn-secondary rounded-full w-9 h-9 flex items-center justify-center"
                aria-label="User preferences"
                title="User preferences"
                onClick={() => setShowPreferences(true)}
              >
                <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="12" r="3" />
                  <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
                </svg>
              </button>
            ) : null}

            {isAuthenticated ? (
              <button
                className="rounded-md px-3 py-2 text-sm text-white"
                style={{ background: "var(--muted-color)" }}
                onClick={logout}
              >
                Sign out
              </button>
            ) : (
              <button
                className="rounded-md px-3 py-2 text-sm text-white"
                style={{ background: "var(--accent-color)" }}
                onClick={login}
              >
                Sign in
              </button>
            )}
          </div>
        </div>
      </header>

      {/* User Preferences Modal */}
      <Modal
        isOpen={showPreferences}
        onClose={() => setShowPreferences(false)}
        title="User Preferences"
        maxWidth="sm"
      >
        <div className="flex items-center justify-between py-2">
          <span style={{ color: "var(--text-color)" }}>
            {theme === "dark" ? "🌙 Dark mode" : "☀️ Light mode"}
          </span>
          <button
            type="button"
            role="switch"
            aria-checked={theme === "dark"}
            className="relative inline-flex h-7 w-12 items-center rounded-full transition-colors"
            style={{ background: theme === "dark" ? "var(--accent-color)" : "var(--line-color)" }}
            onClick={toggleTheme}
            aria-label="Toggle dark mode"
          >
            <span
              className="inline-block h-5 w-5 rounded-full bg-white shadow transition-transform"
              style={{ transform: theme === "dark" ? "translateX(22px)" : "translateX(2px)" }}
            />
          </button>
        </div>
      </Modal>
    </>
  );
}

export const NavBar = Navbar;

