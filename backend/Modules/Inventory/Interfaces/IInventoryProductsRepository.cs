using Csinv.InventoryProducts.DTOs;

namespace Csinv.InventoryProducts.Interfaces;
// Interface for product repository operations
public interface IInventoryProductsRepository
{
    Task<bool> InventoryInsertProduct(string productCode, int productQuantity, int sessionId, int userId);
    Task<List<ProductsFilterResponse>> GetProductsByFilter(ProductsFilterRequest filter);
    Task<List<ProductsDetailsResponse>> GetProductsDetailsByCode(string code);
    Task<List<ProductsDetailsResponse>> GetSessionProducts(int sessionId, SessionProductsFilterRequest filter);
    Task<bool> DeleteProductById(int productId);
}