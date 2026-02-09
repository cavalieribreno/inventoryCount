import { useState, useEffect } from 'react';
import type { Product, ProductDetails } from "./Models/ProductModel";

function ListProducts(){
    // State variables for products list, filters and pagination
    const [products, setProducts] = useState<Product[]>([]);
    const [filterName, setFilterName] = useState("");
    const [filterCode, setFilterCode] = useState("");
    const [filterYear, setFilterYear] = useState ("");
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

            // fetch products from backend API with query string parameters
            const response = await fetch("http://localhost:5144/api/products/filter?" + parameters);
            if(response.ok){
                const productsData = await response.json();
                setProducts(productsData);
            }
        } catch {
            setErrorMsg("Erro ao buscar produtos")
        }
    }
    useEffect(() => {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        fetchProducts(page);
    }, []);

    // Handler for filter, and handlers for pagination buttons (next and previous)
    const handleFilter = () => {
        setPage(1);
        fetchProducts(1);
    }
    const handleNextPage = () => {
        const nextPage = page + 1;
        setPage(nextPage);
        fetchProducts(nextPage);
    }
    const handlePrevPage = () => {
        const prevPage = page - 1;
        setPage(prevPage);
        fetchProducts(prevPage);
    }
    // handle for show details of products
    const handleDetails = async (code: string) => {
        try{
            const response = await fetch(`http://localhost:5144/api/products/details/${code}`);
            if(response.ok){
                const detailsData = await response.json();
                // se nao tiver produto, nao abre o popup
                if(detailsData.length === 0){
                    setShowPopup(false);
                    return;
                }
                setDetails(detailsData);
                setShowPopup(true);
            }
        } catch {
            setErrorMsg("Erro ao buscar detalhes");
        }
    }
    const handleDelete = async (id: number, code: string) => {
        try{
            const response = await fetch(`http://localhost:5144/api/products/delete/${id}`, {
                method: "DELETE"
            });
            if(response.ok){ // comandos para atualizar a lista
                fetchProducts(page) // página principal
                handleDetails(code); //detalhes  
            }
        } catch{
            setErrorMsg("Erro ao excluir produto");
        }
    }
    return (
        <div>
            <h1>Lista de Produtos</h1>
            <div className="filters">
                <input type="text" placeholder="Nome" value={filterName} onChange={(event) => setFilterName(event.target.value)}/>
                <input type="text" placeholder="Código" value={filterCode} onChange={(event) => setFilterCode(event.target.value)}/>
                <input type="text" placeholder="Ano" value={filterYear} onChange={(event) => setFilterYear(event.target.value)}/>
                <button onClick={handleFilter}>Filtrar</button>
            </div>
            {errorMsg && <p>{errorMsg}</p>}
            <table>
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>Código</th>
                        <th>Quantidade</th>
                        <th>Ano</th>
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
            {showPopup /*showPopup true or false (mostra e fecha)*/ && (
                <div className="popup-overlay">
                    <div className="popup">
                        <h2>Detalhes do produto</h2>
                        <table>
                            <thead>
                                <tr>
                                    <th>Id</th>
                                    <th>Quantidade</th>
                                    <th>Ano</th>
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