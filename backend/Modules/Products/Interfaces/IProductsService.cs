using Csinv.Products.DTOs;

namespace Csinv.Products.Interfaces;
// Interface for product service operations
public interface IProductsService
{
    bool ValidateProduct(string productCode, int productQuantity, int year);
    Task<bool> InsertProduct(string productCode, int productQuantity, int year);
    Task<List<ProductsFilterResponse>> GetProductsByFilter(ProductsFilterRequest filter);
    Task<List<ProductsDetailsResponse>> GetProductsDetailsByCode(string code);
    Task<bool> DeleteProductById(int productId);
}