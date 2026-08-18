import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/auth";

export function Register() {
  const [companyName, setCompanyName] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const token = await register(companyName, fullName, email, password);
      localStorage.setItem("token", token);
      navigate("/tickets");
    } catch {
      setError("Kayıt sırasında bir hata oluştu.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h2>NvnDesk Kayıt Ol</h2>
        <p className="auth-subtitle">Şirketini ekle, hemen başla</p>

        {error && <div className="error-banner">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="companyName">Şirket Adı</label>
            <input
              id="companyName"
              className="input-field"
              placeholder="Şirket Adı"
              value={companyName}
              onChange={(e) => setCompanyName(e.target.value)}
              required
            />
          </div>

          <div className="form-field">
            <label htmlFor="fullName">Ad Soyad</label>
            <input
              id="fullName"
              className="input-field"
              placeholder="Ad Soyad"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              required
            />
          </div>

          <div className="form-field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              className="input-field"
              placeholder="ornek@sirket.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="form-field">
            <label htmlFor="password">Şifre</label>
            <input
              id="password"
              type="password"
              className="input-field"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? "Kayıt yapılıyor..." : "Kayıt Ol"}
          </button>
        </form>

        <p className="auth-footer">
          Zaten hesabın var mı? <Link to="/login">Giriş yap</Link>
        </p>
      </div>
    </div>
  );
}