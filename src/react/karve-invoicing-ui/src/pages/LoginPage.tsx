import { useAuth } from "../hooks/useAuth";

export function LoginPage() {
  const { login } = useAuth();

  return (
    <div className="login-page">
      <h1>Karve Invoicing</h1>
      <p>Sign in with your Microsoft account to continue.</p>
      <button className="btn btn-primary" onClick={login}>
        Sign in
      </button>
    </div>
  );
}
