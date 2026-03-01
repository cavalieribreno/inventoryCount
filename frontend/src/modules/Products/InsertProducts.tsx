import { useState } from 'react';
import '../../App.css';

function InsertProducts() {
  // states to save form data
  const [code, setCode] = useState("");
  const [quantity, setQuantity] = useState(0);
  const [session, setSessionId] = useState(0);

  // function to handle form submission and send data to backend
  const handleSubmit = async (event: React.SyntheticEvent) => {
    event.preventDefault();
    try{
      const response = await fetch("http://localhost:5144/api/products/insert", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          code: code,
          quantity: quantity,
          sessionId: session
        })
      });
      if (response.ok) {
        alert("Produto cadastrado com sucesso!");
        setCode("");
        setQuantity(0);
        setSessionId(0);
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
      <form onSubmit={handleSubmit}>
        <div>
          <label> Código:</label>
          <input type="text" value={code} onChange={(event) => setCode(event.target.value)}/>
        </div>
        <div>
          <label> Quantidade:</label>
          <input type="number" value={quantity} onChange={(event) => setQuantity(Number(event.target.value))}/>
        </div>
        <div>
          <label> Sessão:</label>
          <input type="number" value={session}onChange={(event) => setSessionId(Number(event.target.value))}/>
        </div>
        <button type="submit">Cadastrar</button>
      </form>
    </div>
  )
}
export default InsertProducts;
