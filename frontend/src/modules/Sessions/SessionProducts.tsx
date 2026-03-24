import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import type { ProductDetails, GroupedProduct } from '../Products/Models/ProductModel';
import type { Session } from './Models/SessionModel';
import { apiFetch } from '../../services/api';

const monthNames = ["", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];

function SessionProducts() {
    const { sessionId } = useParams();

    // state variables for session info, products list and form
    const [session, setSession] = useState<Session | null>(null);
    const [groupedProducts, setGroupedProducts] = useState<GroupedProduct[]>([]);
    const [modalCode, setModalCode] = useState<string | null>(null);
    const [detailProducts, setDetailProducts] = useState<ProductDetails[]>([]);
    const [errorMsg, setErrorMsg] = useState("");
    // state variables for insert form
    const [code, setCode] = useState("");
    const [quantity, setQuantity] = useState(0);
    // state variables for filters and pagination
    const [filterName, setFilterName] = useState("");
    const [filterCode, setFilterCode] = useState("");
    const [page, setPage] = useState(1);

    // function to fetch session info by id
    const fetchSession = async () => {
        try {
            const response = await apiFetch(`${import.meta.env.VITE_API_URL}/api/sessions/${sessionId}`);
            if (response.ok) {
                const data: Session = await response.json();
                setSession(data);
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar sessão"));
        }
    }

    // function to fetch grouped products of this session
    const fetchGroupedProducts = async (pageNumber: number) => {
        try {
            const parameters = new URLSearchParams();
            parameters.append("page", pageNumber.toString());
            parameters.append("pageSize", "10");
            if (filterName) parameters.append("productName", filterName);
            if (filterCode) parameters.append("code", filterCode);

            const response = await apiFetch(`${import.meta.env.VITE_API_URL}/api/products/session/${sessionId}/grouped?` + parameters);
            if (response.ok) {
                const data = await response.json();
                setGroupedProducts(data);
                setPage(pageNumber);
            } else {
                throw new Error("Erro ao buscar produtos da sessão");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar produtos"));
        }
    }

    // function to open modal with product details
    const handleOpenModal = async (code: string) => {
        try {
            const response = await apiFetch(`${import.meta.env.VITE_API_URL}/api/products/session/${sessionId}/details/${code}`);
            if (response.ok) {
                const data = await response.json();
                setDetailProducts(data);
                setModalCode(code);
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar detalhes"));
        }
    }

    const handleCloseModal = () => {
        setModalCode(null);
        setDetailProducts([]);
    }

    useEffect(() => {
        fetchSession();
        fetchGroupedProducts(page);
    }, [sessionId]);

    // handler to insert a product into this session
    const handleInsert = async (event: React.SyntheticEvent) => {
        event.preventDefault();
        setErrorMsg("");
        try {
            const response = await apiFetch(`${import.meta.env.VITE_API_URL}/api/products/insert`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ code, quantity, sessionId: Number(sessionId) })
            });
            if (response.ok) {
                setCode("");
                setQuantity(0);
                fetchGroupedProducts(page);
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
            const response = await apiFetch(`${import.meta.env.VITE_API_URL}/api/products/delete/${productId}`, {
                method: "DELETE"
            });
            if (response.ok) {
                fetchGroupedProducts(page);
                if (modalCode) handleOpenModal(modalCode);
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
            <Link to="/">&larr; Voltar</Link>

            <h1>Inventário: {sessionTitle}</h1>

            {session && (
                <div className="filters">
                    <p>
                        <strong>Status:</strong> {session.status} | <strong>Início:</strong> {new Date(session.startDate).toLocaleString()} | <strong>Criado por:</strong> {session.createdByName} | <strong>Total itens:</strong> {session.totalItems}
                        {session.status === "finished" && session.finishDate && (
                            <> | <strong>Finalizado em:</strong> {new Date(session.finishDate).toLocaleString()} | <strong>Finalizado por:</strong> {session.finishedByName}</>
                        )}
                        {session.status === "canceled" && session.cancelDate && (
                            <> | <strong>Cancelado em:</strong> {new Date(session.cancelDate).toLocaleString()} | <strong>Cancelado por:</strong> {session.canceledByName}</>
                        )}
                    </p>
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

            <div className="filters">
                <input type="text" placeholder="Nome" value={filterName} onChange={(e) => setFilterName(e.target.value)} />
                <input type="text" placeholder="Código" value={filterCode} onChange={(e) => setFilterCode(e.target.value)} />
                <button onClick={() => fetchGroupedProducts(1)}>Filtrar</button>
            </div>

            {groupedProducts.length === 0 ? (
                <p className="empty-message">Nenhum produto encontrado.</p>
            ) : (
                <>
                    <table>
                        <thead>
                            <tr>
                                <th>Nome</th>
                                <th>Código</th>
                                <th>Qtd Total</th>
                                <th>Ações</th>
                            </tr>
                        </thead>
                        <tbody>
                            {groupedProducts.map((group) => (
                                <tr key={group.code}>
                                    <td>{group.productName}</td>
                                    <td>{group.code}</td>
                                    <td>{group.totalQuantity}</td>
                                    <td>
                                        <button className="btn-details" onClick={() => handleOpenModal(group.code)}>
                                            Detalhes
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <div className="pagination">
                        <button onClick={() => fetchGroupedProducts(page - 1)} disabled={page === 1}>Anterior</button>
                        <span>Página {page}</span>
                        <button onClick={() => fetchGroupedProducts(page + 1)} disabled={groupedProducts.length < 10}>Próxima</button>
                    </div>
                </>
            )}

            {modalCode && (
                <div className="modal-overlay" onClick={handleCloseModal}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <h3>Detalhes — {detailProducts[0]?.productName} ({modalCode})</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>Quantidade</th>
                                    <th>Data</th>
                                    <th>Inserido por</th>
                                    {session?.status === "active" && <th>Ações</th>}
                                </tr>
                            </thead>
                            <tbody>
                                {detailProducts.map((detail) => (
                                    <tr key={detail.id}>
                                        <td>{detail.quantity}</td>
                                        <td>{new Date(detail.dateHour).toLocaleString()}</td>
                                        <td>{detail.userName}</td>
                                        {session?.status === "active" && (
                                            <td><button onClick={() => handleDelete(detail.id)}>Excluir</button></td>
                                        )}
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        <button className="btn-details" onClick={handleCloseModal}>Fechar</button>
                    </div>
                </div>
            )}
        </div>
    )
}

export default SessionProducts;
