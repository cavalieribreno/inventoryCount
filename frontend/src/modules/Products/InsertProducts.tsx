import { useState, useEffect } from 'react';
import '../../App.css';

const monthNames = ["", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];

function InsertProducts() {
  // states to save form data
  const [code, setCode] = useState("");
  const [quantity, setQuantity] = useState(0);
  // state for active session
  const [activeSession, setActiveSession] = useState<{ id: number; year: number; month: number | null } | null>(null);

  // function to fetch the active session from backend
  const fetchActiveSession = async () => {
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/active`);
      if (response.ok) {
        const data = await response.json();
        setActiveSession(data);
      } else {
        setActiveSession(null);
      }
    } catch (err) {
      console.error("Erro ao buscar sessão ativa", err);
    }
  }

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchActiveSession();
  }, []);

  // function to handle form submission and send data to backend
  const handleSubmit = async (event: React.SyntheticEvent) => {
    event.preventDefault();
    if (!activeSession) return;
    try{
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/insert`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          code: code,
          quantity: quantity,
          sessionId: activeSession.id
        })
      });
      if (response.ok) {
        alert("Produto cadastrado com sucesso!");
        setCode("");
        setQuantity(0);
      } else {
        alert("Erro ao cadastrar produto.");
      }
    } catch (error) {
      console.error("Erro de conexão com o servidor", error);
    }
  }
  // form to collect product data and submit
  return (
    <div>
      <h1>Cadastro de Produto</h1>

      {activeSession ? (
        <>
          <p><strong>Sessão ativa:</strong> #{activeSession.id} — {activeSession.month != null ? monthNames[activeSession.month] + "/" : ""}{activeSession.year}</p>
          <form onSubmit={handleSubmit}>
            <div>
              <label> Código:</label>
              <input type="text" value={code} onChange={(event) => setCode(event.target.value)}/>
            </div>
            <div>
              <label> Quantidade:</label>
              <input type="number" value={quantity} onChange={(event) => setQuantity(Number(event.target.value))}/>
            </div>
            <button type="submit">Cadastrar</button>
          </form>
        </>
      ) : (
        <p>Nenhum inventário ativo. Crie um inventário antes de cadastrar produtos.</p>
      )}
    </div>
  )
}
export default InsertProducts;
