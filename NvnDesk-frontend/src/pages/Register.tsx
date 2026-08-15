import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/auth";

export function Register() {
  const [companyName, setCompanyName] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    try {
      const token = await register(companyName, fullName, email, password);
      localStorage.setItem("token", token);
      navigate("/tickets");
    } catch {
      setError("Kayıt sırasında bir hata oluştu.");
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>NvnDesk Kayıt Ol</h2>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <input
        placeholder="Şirket Adı"
        value={companyName}
        onChange={(e) => setCompanyName(e.target.value)}
        required
      />
      <input
        placeholder="Ad Soyad"
        value={fullName}
        onChange={(e) => setFullName(e.target.value)}
        required
      />
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
      <button type="submit">Kayıt Ol</button>
      <p>
        Zaten hesabın var mı? <Link to="/login">Giriş yap</Link>
      </p>
    </form>
  );
}