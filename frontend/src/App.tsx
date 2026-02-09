import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import InsertProducts from './modules/Products/InsertProducts';
import ListProducts from './modules/Products/ListProducts';

function App() {
  // routes for listing and inserting products
  return (
    <BrowserRouter>
      <nav>
        <Link to="/">Listar Produtos</Link>
        <Link to="/insert">Cadastrar Produtos</Link>
      </nav>

      <Routes>
        <Route path="/" element={<ListProducts/>}/>
        <Route path="/insert" element={<InsertProducts/>}/>
      </Routes>
    </BrowserRouter>
  )
}

export default App;
