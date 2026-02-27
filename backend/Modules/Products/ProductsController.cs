using Csinv.Products.DTOs;
using Csinv.Products.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Csinv.Products.Controller;

[ApiController]
[Route("api/products")]
// Controller class for product-related endpoints
public class ProductsController : ControllerBase
{
    // Dependency injection of the products service
    private readonly IProductsService _productsService;
    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }
    // Endpoint to insert a new product
    [HttpPost("insert")]
    public async Task<IActionResult> InsertProduct([FromBody] ProductsInsertRequest productRequest)
    {
        if (!_productsService.ValidateProduct(productRequest.Code!, productRequest.Quantity, productRequest.Year))
        {
            return BadRequest("Invalid product code, quantity or year.");
        }
        var result = await _productsService.InsertProduct(productRequest.Code!, productRequest.Quantity, productRequest.Year);
        if (result)
        {
            return Ok("Product inserted successfully.");
        }
        else
        {
            return StatusCode(500, "An error occurred while inserting the product.");
        }
    }
    // Endpoint to get products based on filter 
    [HttpGet("filter")]
    public async Task<IActionResult> GetProductsByFilter([FromQuery] ProductsFilterRequest filter)
    {
        var products = await _productsService.GetProductsByFilter(filter);
        if(products == null || products.Count == 0)
        {
            return NotFound("No products found matching the filter criteria.");
        }
        return Ok(products);
    }
    // Endpoint to get product details by code
    [HttpGet("details/{code}")]
    public async Task<IActionResult> GetProductsDetailsByCode(string code)
    {
        var products = await _productsService.GetProductsDetailsByCode(code);
        if(products == null || products.Count == 0)
        {
            return NotFound("No products found with the specified code.");
        }
        return Ok(products);
    }
    // Endpoint to delete product by id
    [HttpDelete("delete/{productId}")]
    public async Task<IActionResult> DeleteProductById(int productId)
    {
        bool productDelete = await _productsService.DeleteProductById(productId);
        if (productDelete)
        {
            return Ok(productDelete);
        }
        else
        {
            return StatusCode(500, "An error occurred to delete product.");
        }
    }
}