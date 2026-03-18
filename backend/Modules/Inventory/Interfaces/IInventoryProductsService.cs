using Csinv.InventoryProducts.DTOs;

namespace Csinv.InventoryProducts.Interfaces;
// Interface for product service operations
public interface IInventoryProductsService
{
    bool ValidateProduct(string productCode, int productQuantity, int sessionId);
    Task<bool> InventoryInsertProduct(string productCode, int productQuantity, int sessionId, int userId);
    Task<List<SessionGroupedProductsResponse>> GetSessionGroupedProducts(int sessionId, SessionProductsFilterRequest filter);
    Task<List<ProductsDetailsResponse>> GetProductsDetails(string code, int sessionId);
    Task<bool> DeleteProductById(int productId);
}