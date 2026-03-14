import { useAuth } from "../hooks/useAuth";
import { useCurrentUser } from "../hooks/useCurrentUser";

export function Navbar() {
  const { isAuthenticated, login, logout } = useAuth();
  const {
    profile,
    memberships,
    selectedCompanyId,
    setSelectedCompanyId,
    isLoading,
  } = useCurrentUser();

  return (
    <header className="sticky top-0 z-20 border-b border-[color:var(--line)] bg-[color:var(--paper)]/90 backdrop-blur">
      <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6 lg:px-8">
        <div>
          <h1 className="font-heading text-lg tracking-tight text-[color:var(--ink)]">
            Karve Invoicing
          </h1>
          {isAuthenticated && profile.email ? (
            <p className="text-xs text-[color:var(--muted)]">{profile.email}</p>
          ) : null}
        </div>

        <div className="flex items-center gap-3">
          {isAuthenticated && memberships.length > 1 ? (
            <label className="flex items-center gap-2 text-sm text-[color:var(--ink)]">
              <span>Company</span>
              <select
                className="rounded-md border border-[color:var(--line)] bg-white px-2 py-1 text-sm"
                value={selectedCompanyId ?? ""}
                onChange={(event) => setSelectedCompanyId(event.target.value)}
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

          {isAuthenticated ? (
            <button
              className="rounded-md bg-slate-900 px-3 py-2 text-sm text-white hover:bg-slate-700"
              onClick={logout}
            >
              Sign out
            </button>
          ) : (
            <button
              className="rounded-md bg-[color:var(--accent)] px-3 py-2 text-sm text-white hover:brightness-95"
              onClick={login}
            >
              Sign in
            </button>
          )}
        </div>
      </div>
    </header>
  );
}

export const NavBar = Navbar;
