using Csinv.Products.DTOs;
using Csinv.Products.Interfaces;

namespace Csinv.Products.Service;
// Service class for product operations
public class ProductsService : IProductsService
{
    // Dependency injection of the products repository
    private readonly IProductsRepository _productsrepository;
    public ProductsService(IProductsRepository productsrepository)
    {
        _productsrepository = productsrepository;
    }
    // Method to validate product details
    public bool ValidateProduct(string productCode, int productQuantity, int year)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productQuantity <= 0 || year <= 0)
        {
            return false;
        }
        return true;
    }
    // Method to insert a product using the repository
    public async Task<bool> InsertProduct(string productCode, int productQuantity, int year)
    {
        var result = await _productsrepository.InsertProduct(productCode, productQuantity, year);
        return result;
    }
    // Method to get products by filter using the repository
    public async Task<List<ProductsFilterResponse>> GetProductsByFilter(ProductsFilterRequest filter)
    {
        var products = await _productsrepository.GetProductsByFilter(filter);
        return products;
    }
    // Method to get product details by code
    public async Task<List<ProductsDetailsResponse>> GetProductsDetailsByCode(string code)
    {
        var products = await _productsrepository.GetProductsDetailsByCode(code);
        return products;
    }
    // Method do delete product by id
    public async Task<bool> DeleteProductById(int productId)
    {
        bool productDelete = await _productsrepository.DeleteProductById(productId);
        return productDelete;
    }
}