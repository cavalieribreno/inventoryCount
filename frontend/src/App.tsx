import { BrowserRouter, Routes, Route, Link, useLocation } from 'react-router-dom';
import InsertProducts from './modules/Products/InsertProducts';
import ListProducts from './modules/Products/ListProducts';
import Sessions from './modules/Sessions/Sessions';
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
          <Route path="/insert" element={<InsertProducts/>}/>
          <Route path="/sessions" element={<Sessions/>}/>
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
            to="/insert"
            className={`nav-link ${location.pathname === '/insert' ? 'active' : ''}`}
          >
            Novo Produto
          </Link>
          <Link
            to="/sessions"
            className={`nav-link ${location.pathname === '/sessions' ? 'active' : ''}`}
          >
            Inventários
          </Link>
        </nav>
      </div>
    </header>
  )
}

export default App;
