import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

function Login() {
    // state variables for form data and error message
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMsg, setErrorMsg] = useState("");
    const navigate = useNavigate();
    const { login } = useAuth();

    // function to handle login form submission
    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMsg("");
        try {
            // send login request to backend API
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/users/login`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });
            if (response.ok) {
                const data = await response.json();
                login(data); // save user data in context and localStorage
                navigate("/"); // redirect to home page
            } else {
                setErrorMsg("Email ou senha inválidos.");
            }
        } catch {
            setErrorMsg("Erro ao conectar com o servidor.");
        }
    };
    // login form
    return (
        <div className="login-container">
            <h1>Login</h1>
            <form onSubmit={handleLogin}>
                <div>
                    <label>Email:</label>
                    <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
                </div>
                <div>
                    <label>Senha:</label>
                    <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
                </div>
                <button type="submit">Entrar</button>
            </form>
            {errorMsg && <p className="error-message">{errorMsg}</p>}
        </div>
    );
}

export default Login;
