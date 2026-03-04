import { useState, useEffect } from 'react';
import type { Session } from './Models/SessionModel';

const monthNames = ["", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];

function Sessions() {
    // state variables for sessions list, active session, new session year and errors
    const [sessions, setSessions] = useState<Session[]>([]);
    const [activeSession, setActiveSession] = useState<Session | null>(null);
    const [year, setYear] = useState("");
    const [month, setMonth] = useState("");
    const [errorMsg, setErrorMsg] = useState("");

    // function to fetch the current active session from backend
    const fetchActiveSession = async () => {
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/active`);
            if (response.ok) {
                const data = await response.json();
                setActiveSession(data);
            } else {
                setActiveSession(null);
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar sessão ativa"));
        }
    }

    // function to fetch all sessions from backend
    const fetchAllSessions = async () => {
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/getall`);
            if (response.ok) {
                const data = await response.json();
                setSessions(data);
            } else {
                throw new Error("Erro ao buscar sessões");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar sessões"));
        }
    }

    useEffect(() => {
        fetchActiveSession();
        fetchAllSessions();
    }, []);

    // handler to create a new inventory session
    const handleCreate = async (event: React.SyntheticEvent) => {
        event.preventDefault();
        setErrorMsg("");
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/create`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ year: Number(year), month: month ? Number(month) : null })
            });
            if (response.ok) {
                fetchActiveSession();
                fetchAllSessions();
            } else {
                const msg = await response.text();
                setErrorMsg(msg || "Erro ao criar inventário");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao criar sessão"));
        }
    }

    // handler to finish the active session
    const handleFinish = async (sessionId: number) => {
        setErrorMsg("");
        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/${sessionId}/finish`, {
                method: "PATCH"
            });
            if (response.ok) {
                fetchActiveSession();
                fetchAllSessions();
            } else {
                const msg = await response.text();
                setErrorMsg(msg || "Erro ao finalizar inventário");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao finalizar sessão"));
        }
    }
    // handler to cancel the active session
    const handleCancel = async (sessionId: number) => {
        setErrorMsg("");
        try{
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/sessions/${sessionId}/cancel`, {
                method: "PATCH"
            });
            if (response.ok){
                fetchActiveSession();
                fetchAllSessions();
            } else {
                const msg = await response.text();
                setErrorMsg(msg || "Erro ao cancelar inventário");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao cancelar sessão"));
        }
    }

    return (
        <div>
            <h1>Inventários</h1>

            <div className="filters">
                <div>
                    <strong>Inventário ativo: </strong>
                    {activeSession
                        ? <span>#{activeSession.id} — {activeSession.month ? monthNames[activeSession.month] + "/" : "Anual — "}{activeSession.year} (iniciado em {new Date(activeSession.startDate).toLocaleString()})</span>
                        : <span>Nenhum inventário ativo</span>
                    }
                    {activeSession && (
                        <>
                            <button onClick={() => handleFinish(activeSession.id)} style={{ marginLeft: "12px" }}>
                                Finalizar
                            </button>
                            <button onClick={() => handleCancel(activeSession.id)} style={{ marginLeft: "8px" }}>
                                Cancelar
                            </button>
                        </>
                    )}
                </div>
            </div>

            {!activeSession && (
                <div className="filters">
                    <form onSubmit={handleCreate}>
                        <select value={month} onChange={(e) => setMonth(e.target.value)}>
                            <option value="">Mês</option>
                            <option value="1">Janeiro</option>
                            <option value="2">Fevereiro</option>
                            <option value="3">Março</option>
                            <option value="4">Abril</option>
                            <option value="5">Maio</option>
                            <option value="6">Junho</option>
                            <option value="7">Julho</option>
                            <option value="8">Agosto</option>
                            <option value="9">Setembro</option>
                            <option value="10">Outubro</option>
                            <option value="11">Novembro</option>
                            <option value="12">Dezembro</option>
                        </select>
                        <input
                            type="number"
                            placeholder="Ano"
                            value={year}
                            onChange={(e) => setYear(e.target.value)}
                        />
                        <button type="submit">Novo Inventário</button>
                    </form>
                </div>
            )}

            {errorMsg && <p className="error-message">{errorMsg}</p>}

            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Ano</th>
                        <th>Mês</th>
                        <th>Status</th>
                        <th>Início</th>
                        <th>Fim</th>
                        <th>Cancelado em</th>
                        <th>Total Itens</th>
                    </tr>
                </thead>
                <tbody>
                    {sessions.map((session) => (
                        <tr key={session.id}>
                            <td>{session.id}</td>
                            <td>{session.year}</td>
                            <td>{session.month ? monthNames[session.month] : "Anual"}</td>
                            <td>{session.status}</td>
                            <td>{new Date(session.startDate).toLocaleString()}</td>
                            <td>{session.finishDate ? new Date(session.finishDate).toLocaleString() : "—"}</td>
                            <td>{session.cancelDate ? new Date(session.cancelDate).toLocaleString() : "—"}</td>
                            <td>{session.totalItems}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default Sessions;
