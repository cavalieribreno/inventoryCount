import { BrowserRouter, Routes, Route, Link, useLocation, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Sessions from './modules/Sessions/Sessions';
import SessionProducts from './modules/Sessions/SessionProducts';
import Login from './modules/Auth/Login';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppLayout />
      </AuthProvider>
    </BrowserRouter>
  )
}

function AppLayout() {
  const { user } = useAuth();

  if (!user) return <Routes><Route path="*" element={<Login />} /></Routes>;

  return (
    <div className="app-wrapper">
      <Header />
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Sessions/>}/>
          <Route path="/sessions/:sessionId" element={<SessionProducts/>}/>
          <Route path="*" element={<Navigate to="/" />}/>
        </Routes>
      </main>
    </div>
  )
}

function Header() {
  const location = useLocation();
  const { logout, user } = useAuth();
  return (
    <header className="app-header">
      <div className="header-container">
        <div className="logo-section">
          <h1 className="logo">📦 Inventário</h1>
          <p className="tagline">Cacau Show</p>
        </div>
        <nav className="app-nav">
          <Link
            to="/"
            className={`nav-link ${location.pathname === '/' || location.pathname.startsWith('/sessions') ? 'active' : ''}`}
          >
            Inventários
          </Link>
          <div className="profile-wrapper">
            <span className="nav-link profile-link">
              {user?.name}
            </span>
            <div className="profile-popup">
              <p><strong>Nome:</strong> {user?.name}</p>
              <p><strong>Email:</strong> {user?.email}</p>
            </div>
          </div>
          <button className="nav-logout" onClick={logout}>Sair</button>
        </nav>
      </div>
    </header>
  )
}

export default App;
