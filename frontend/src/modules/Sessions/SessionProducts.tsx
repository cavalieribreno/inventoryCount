import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import type { ProductDetails } from '../Products/Models/ProductModel';
import type { Session } from './Models/SessionModel';

const monthNames = ["", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];

function SessionProducts() {
    const { sessionId } = useParams();

    // state variables for session info, products list and form
    const [session, setSession] = useState<Session | null>(null);
    const [products, setProducts] = useState<ProductDetails[]>([]);
    const [errorMsg, setErrorMsg] = useState("");
    // state variables for insert form
    const [code, setCode] = useState("");
    const [quantity, setQuantity] = useState(0);

    // function to fetch session info from all sessions
    const fetchSession = async () => {
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/getall`);
            if (response.ok) {
                const sessions: Session[] = await response.json();
                const found = sessions.find(s => s.id === Number(sessionId));
                setSession(found || null);
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar sessão"));
        }
    }

    // function to fetch products of this session
    const fetchProducts = async () => {
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/session/${sessionId}`);
            if (response.ok) {
                const data = await response.json();
                setProducts(data);
            } else if (response.status === 404) {
                setProducts([]);
            } else {
                throw new Error("Erro ao buscar produtos da sessão");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar produtos"));
        }
    }

    useEffect(() => {
        fetchSession();
        fetchProducts();
    }, [sessionId]);

    // handler to insert a product into this session
    const handleInsert = async (event: React.SyntheticEvent) => {
        event.preventDefault();
        setErrorMsg("");
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/insert`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ code, quantity, sessionId: Number(sessionId) })
            });
            if (response.ok) {
                setCode("");
                setQuantity(0);
                fetchProducts();
            } else {
                const msg = await response.text();
                setErrorMsg(msg || "Erro ao inserir produto");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao inserir produto"));
        }
    }

    // handler to delete a product entry
    const handleDelete = async (productId: number) => {
        setErrorMsg("");
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/delete/${productId}`, {
                method: "DELETE"
            });
            if (response.ok) {
                fetchProducts();
            } else {
                throw new Error("Erro ao excluir produto");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao excluir produto"));
        }
    }

    // session title: month/year or Anual — year
    const sessionTitle = session
        ? session.month ? `${monthNames[session.month]}/${session.year}` : `Anual — ${session.year}`
        : "";

    return (
        <div>
            <Link to="/sessions">&larr; Voltar</Link>

            <h1>Inventário: {sessionTitle}</h1>

            {session && (
                <div className="filters">
                    <p><strong>Status:</strong> {session.status} | <strong>Início:</strong> {new Date(session.startDate).toLocaleString()} | <strong>Total itens:</strong> {session.totalItems}</p>
                </div>
            )}

            {session?.status === "active" && (
                <div className="filters">
                    <form onSubmit={handleInsert} style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                        <input type="text" placeholder="Código" value={code} onChange={(e) => setCode(e.target.value)} />
                        <input type="number" placeholder="Quantidade" value={quantity} onChange={(e) => setQuantity(Number(e.target.value))} />
                        <button type="submit">Adicionar</button>
                    </form>
                </div>
            )}

            {errorMsg && <p className="error-message">{errorMsg}</p>}

            <table>
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>Código</th>
                        <th>Quantidade</th>
                        <th>Data</th>
                        {session?.status === "active" && <th>Ações</th>}
                    </tr>
                </thead>
                <tbody>
                    {products.map((product) => (
                        <tr key={product.id}>
                            <td>{product.productName}</td>
                            <td>{product.code}</td>
                            <td>{product.quantity}</td>
                            <td>{new Date(product.dateHour).toLocaleString()}</td>
                            {session?.status === "active" && (
                                <td><button onClick={() => handleDelete(product.id)}>Excluir</button></td>
                            )}
                        </tr>
                    ))}
                </tbody>
            </table>

            {products.length === 0 && <p>Nenhum produto nesta sessão.</p>}
        </div>
    )
}

export default SessionProducts;
