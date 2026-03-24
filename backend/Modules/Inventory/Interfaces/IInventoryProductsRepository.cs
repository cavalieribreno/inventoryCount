using Csinv.InventoryProducts.DTOs;

namespace Csinv.InventoryProducts.Interfaces;
// Interface for product repository operations
public interface IInventoryProductsRepository
{
    Task<bool> InventoryInsertProduct(string productCode, int productQuantity, int sessionId, int userId);
    Task<List<SessionGroupedProductsResponse>> GetSessionGroupedProducts(int sessionId, SessionProductsFilterRequest filter);
    Task<bool> ProductExistsByCode(string code);
    Task<List<ProductsDetailsResponse>> GetProductsDetails(string code, int sessionId);
    Task<bool> DeleteProductById(int productId);
}