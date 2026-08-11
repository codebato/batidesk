import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../api/auth";

export function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault(); 
    setError(null);

    try {
      const token = await login(email, password);

      localStorage.setItem("token", token);
      navigate("/tickets"); 
    } catch {
      setError("Email veya şifre hatalı.");
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>WestDesk Giriş</h2>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <input
        type="email"
        placeholder="Email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        required
      />
      <input
        type="password"
        placeholder="Şifre"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
      />
      <button type="submit">Giriş Yap</button>
    </form>
  );
}