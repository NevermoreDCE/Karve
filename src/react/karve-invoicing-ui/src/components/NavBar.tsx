import { useAuth } from "../hooks/useAuth";

export function NavBar() {
  const { isAuthenticated, login, logout } = useAuth();

  return (
    <nav className="navbar">
      <span className="navbar-brand">Karve Invoicing</span>
      <div className="navbar-actions">
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
