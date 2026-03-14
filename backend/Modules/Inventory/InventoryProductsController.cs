using System.Security.Claims;
using Csinv.InventoryProducts.DTOs;
using Csinv.InventoryProducts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Csinv.InventoryProducts.Controller;

[ApiController]
[Route("api/products")]
[Authorize]
// Controller class for product-related endpoints
public class InventoryProductsController : ControllerBase
{
    // Dependency injection of the products service
    private readonly IInventoryProductsService _productsService;
    public InventoryProductsController(IInventoryProductsService productsService)
    {
        _productsService = productsService;
    }
    // Endpoint to insert a new product
    [HttpPost("insert")]
    public async Task<IActionResult> InventoryInsertProduct([FromBody] ProductsInsertRequest productRequest)
    {
        if (!_productsService.ValidateProduct(productRequest.Code!, productRequest.Quantity, productRequest.SessionId))
        {
            return BadRequest("Invalid product code, quantity or year.");
        }
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _productsService.InventoryInsertProduct(productRequest.Code!, productRequest.Quantity, productRequest.SessionId, userId);
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
    // Endpoint to get products of a session
    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSessionProducts(int sessionId, [FromQuery] SessionProductsFilterRequest filter)
    {
        try
        {
            var products = await _productsService.GetSessionProducts(sessionId, filter);
            return Ok(products);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
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