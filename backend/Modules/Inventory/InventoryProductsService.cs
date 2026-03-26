using Csinv.InventoryProducts.DTOs;
using Csinv.InventoryProducts.Interfaces;
using Csinv.InventorySessions.Interfaces;

namespace Csinv.InventoryProducts.Service;
// Service class for product operations
public class InventoryProductsService : IInventoryProductsService
{
    // Dependency injection of the products repository
    private readonly IInventoryProductsRepository _productsrepository;
    private readonly ISessionRepository _sessionrepository;
    public InventoryProductsService(IInventoryProductsRepository productsrepository, ISessionRepository sessionrepository)
    {
        _productsrepository = productsrepository;
        _sessionrepository = sessionrepository;
    }
    // Method to validate product details
    public bool ValidateProduct(string productCode, int productQuantity)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productQuantity <= 0)
        {
            return false;
        }
        return true;
    }
    // Method to get product from catalog by code
    public async Task<CatalogProductResponse?> GetProductByCode(string code)
    {
        return await _productsrepository.GetProductByCode(code);
    }
    // Method to insert a product using the repository
    public async Task<bool> InventoryInsertProduct(string productCode, int productQuantity, int sessionId, int userId)
    {
        var activeSession = await _sessionrepository.GetActiveSession();
        if(activeSession == null || activeSession.Id != sessionId)
        {
            throw new InvalidOperationException("No active inventory session found for the provided session ID.");
        }
        // check if product exists in products table before inserting
        if(await _productsrepository.GetProductByCode(productCode) == null)
        {
            throw new InvalidOperationException("Product code not found.");
        }
        var result = await _productsrepository.InventoryInsertProduct(productCode, productQuantity, sessionId, userId);
        return result;
    }
    // Method to get grouped products of a session
    public async Task<List<SessionGroupedProductsResponse>> GetSessionGroupedProducts(int sessionId, SessionProductsFilterRequest filter)
    {
        return await _productsrepository.GetSessionGroupedProducts(sessionId, filter);
    }
    // Method to get product details by code and session
    public async Task<List<ProductsDetailsResponse>> GetProductsDetails(string code, int sessionId)
    {
        var products = await _productsrepository.GetProductsDetails(code, sessionId);
        return products;
    }
    // Method do delete product by id
    public async Task<bool> DeleteProductById(int productId)
    {
        bool productDelete = await _productsrepository.DeleteProductById(productId);
        return productDelete;
    }
}