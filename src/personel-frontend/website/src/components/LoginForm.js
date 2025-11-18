import { useState } from 'react';
import axiosClient  from '../api/axiosClient';  

external default function LoginForm({ onLoginSuccess }) {
    const [email, setUsername] = useState('');
    const [sifre, setPassword] = useState('');
    const [error, setError] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const res = await axiosClient.post('/Auth/login', { email, sifre });
            localStorage.setItem('token', res.data);
            onLogin();
        } catch (err) {
            setError('Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
        }
    };

    return (
    <div style={{ maxWidth: 300, margin: "80px auto", textAlign: "center" }}>
      <h2>Giriş Yap</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="email"
          placeholder="E-posta"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          style={{ width: "100%", padding: 8 }}
        />
        <br /><br />
        <input
          type="password"
          placeholder="Şifre"
          value={sifre}
          onChange={(e) => setSifre(e.target.value)}
          style={{ width: "100%", padding: 8 }}
        />
        <br /><br />
        <button type="submit" style={{ padding: "8px 20px" }}>Giriş</button>
      </form>
      {error && <p style={{ color: "red" }}>{error}</p>}
    </div>
  );
}