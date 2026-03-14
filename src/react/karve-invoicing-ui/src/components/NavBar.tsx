import { useAuth } from "../hooks/useAuth";
import { useCurrentUser } from "../hooks/useCurrentUser";

export function NavBar() {
  const { isAuthenticated, login, logout } = useAuth();
  const {
    profile,
    memberships,
    selectedCompanyId,
    setSelectedCompanyId,
    isLoading,
  } = useCurrentUser();

  return (
    <nav className="navbar">
      <div>
        <span className="navbar-brand">Karve Invoicing</span>
        {isAuthenticated && profile.email ? (
          <div style={{ fontSize: 12, opacity: 0.8 }}>{profile.email}</div>
        ) : null}
      </div>

      <div className="navbar-actions">
        {isAuthenticated && memberships.length > 1 ? (
          <label style={{ marginRight: 8 }}>
            <span style={{ marginRight: 6 }}>Company</span>
            <select
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
          <button className="btn btn-secondary" onClick={logout}>
            Sign out
          </button>
        ) : (
          <button className="btn btn-primary" onClick={login}>
            Sign in
          </button>
        )}
      </div>
    </nav>
  );
}
