import { useState, useEffect } from 'react';
import type { Product, ProductDetails } from "./Models/ProductModel";

const monthNames = ["", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];

function ListProducts(){
    // State variables for products list, filters and pagination
    const [products, setProducts] = useState<Product[]>([]);
    const [filterName, setFilterName] = useState("");
    const [filterCode, setFilterCode] = useState("");
    const [filterYear, setFilterYear] = useState ("");
    const [filterMonth, setFilterMonth] = useState("");
    const [page, setPage] = useState(1);
    // errors
    const [errorMsg, setErrorMsg] = useState("");
    // state variables for show details of products
    const [details, setDetails] = useState<ProductDetails[]>([]);
    const [showPopup, setShowPopup] = useState(false);

    // Function to fetch products from backend with filters and pagination
    const fetchProducts = async (pageNumber: number) => {
        setErrorMsg("");
        try{
            const parameters = new URLSearchParams();
            parameters.append("page", pageNumber.toString());
            parameters.append("pageSize", "10");
            if(filterName) parameters.append("productName", filterName);
            if(filterCode) parameters.append("code", filterCode);
            if(filterYear) parameters.append("year", filterYear);
            if(filterMonth) parameters.append("month", filterMonth);

            // fetch products from backend API with query string parameters
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/filter?` + parameters);
            if(response.ok){
                const productsData = await response.json();
                if(productsData.length > 0){
                    setProducts(productsData);
                    setPage(pageNumber);
                }
            } else {
                throw new Error("Erro ao buscar produtos");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar produtos"));
        }
    }
    useEffect(() => {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        fetchProducts(page);
    }, []);

    // Handler for filter, and handlers for pagination buttons (next and previous)
    const handleFilter = () => {
        fetchProducts(1);
    }
    const handleNextPage = () => {
        fetchProducts(page + 1);
    }
    const handlePrevPage = () => {
        fetchProducts(page - 1);
    }
    // handle for show details of products
    const handleDetails = async (code: string) => {
        setErrorMsg("");
        try{
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/details/${code}`);
            if(response.ok){
                const detailsData = await response.json();
                // se nao tiver produto, nao abre o popup
                if(detailsData.length === 0){
                    setShowPopup(false);
                    return;
                }
                setDetails(detailsData);
                setShowPopup(true);
            } else {
                throw new Error("Erro ao buscar detalhes");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao buscar detalhes"));
        }
    }
    // handle for delete product details by id, after delete, update details by code again
    const handleDelete = async (id: number, code: string) => {
        setErrorMsg("");
        try{
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/products/delete/${id}`, {
                method: "DELETE"
            });
            if(response.ok){ // comandos para atualizar a lista
                fetchProducts(page) // página principal
                handleDetails(code); //detalhes  
            } else {
                throw new Error("Erro ao excluir produto");
            }
        } catch (err: unknown) {
            setErrorMsg(err instanceof Error ? err.message : String(err ?? "Erro ao excluir produto"));
        }
    }
    return (
        <div>
            <h1>Lista de Produtos</h1>
            <div className="filters">
                <input type="text" placeholder="Nome" value={filterName} onChange={(event) => setFilterName(event.target.value)}/>
                <input type="text" placeholder="Código" value={filterCode} onChange={(event) => setFilterCode(event.target.value)}/>
                <input type="text" placeholder="Ano" value={filterYear} onChange={(event) => setFilterYear(event.target.value)}/>
                <select value={filterMonth} onChange={(e) => setFilterMonth(e.target.value)}>
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
                <button onClick={handleFilter}>Filtrar</button>
            </div>
            {errorMsg && <p className="error-message">{errorMsg}</p>}
            <table>
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>Código</th>
                        <th>Quantidade</th>
                        <th>Ano</th>
                        <th>Mês</th>
                        <th>Ações</th>
                    </tr>
                </thead>
                <tbody>
                    {products.map((product: Product, i: number) => (
                        <tr key={i}>
                            <td>{product.productName}</td>
                            <td>{product.code}</td>
                            <td>{product.totalQuantity}</td>
                            <td>{product.year}</td>
                            <td>{product.month ? monthNames[product.month] : "Anual"}</td>
                            <td><button onClick={() => handleDetails(product.code)}>...</button></td>
                        </tr>
                    ))}
                </tbody>
            </table>
            <div className="pagination">
                <button onClick={handlePrevPage} disabled={page === 1}>Anterior</button>
                <span>Página {page}</span>
                <button onClick={handleNextPage} disabled={products.length < 10}>Próxima</button>
            </div>
            {showPopup /*showPopup (detalhes dos produtos) true or false (mostra e fecha)*/ && (
                <div className="popup-overlay">
                    <div className="popup">
                        <h2>Detalhes do produto</h2>
                        <table>
                            <thead>
                                <tr>
                                    <th>Id</th>
                                    <th>Quantidade</th>
                                    <th>Ano</th>
                                    <th>Mês</th>
                                    <th>Data de Criação</th>
                                    <th>Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {details.map((detail) =>(
                                    <tr key={detail.id}>
                                        <td>{detail.id}</td>
                                        <td>{detail.quantity}</td>
                                        <td>{detail.year}</td>
                                        <td>{detail.month ? monthNames[detail.month] : "Anual"}</td>
                                        <td>{new Date(detail.dateHour).toLocaleString()}</td>
                                        <td><button onClick={() => handleDelete(detail.id, detail.code)}>Excluir</button></td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        <button onClick={() => setShowPopup(false)}>Fechar</button>
                    </div>
                </div>
            )}
        </div>
    )
}
export default ListProducts;