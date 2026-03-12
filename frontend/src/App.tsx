import { BrowserRouter, Routes, Route, Link, useLocation } from 'react-router-dom';
import ListProducts from './modules/Products/ListProducts';
import Sessions from './modules/Sessions/Sessions';
import SessionProducts from './modules/Sessions/SessionProducts';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <AppLayout />
    </BrowserRouter>
  )
}

function AppLayout() {
  return (
    <div className="app-wrapper">
      <Header />
      <main className="main-content">
        <Routes>
          <Route path="/" element={<ListProducts/>}/>
          <Route path="/sessions" element={<Sessions/>}/>
          <Route path="/sessions/:sessionId" element={<SessionProducts/>}/>
        </Routes>
      </main>
    </div>
  )
}

function Header() {
  const location = useLocation();

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
            className={`nav-link ${location.pathname === '/' ? 'active' : ''}`}
          >
            Produtos
          </Link>
          <Link
            to="/sessions"
            className={`nav-link ${location.pathname.startsWith('/sessions') ? 'active' : ''}`}
          >
            Inventários
          </Link>
        </nav>
      </div>
    </header>
  )
}

export default App;
