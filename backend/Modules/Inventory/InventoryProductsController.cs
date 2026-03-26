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
    // Endpoint to get product from catalog by code
    [HttpGet("catalog")]
    public async Task<IActionResult> GetProductByCode([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Product code is required.");
        }
        var product = await _productsService.GetProductByCode(code);
        if (product == null)
        {
            return NotFound("Product not found.");
        }
        return Ok(product);
    }
    // Endpoint to insert a new product
    [HttpPost("insert")]
    public async Task<IActionResult> InventoryInsertProduct([FromBody] ProductsInsertRequest productRequest)
    {
        if (!_productsService.ValidateProduct(productRequest.Code!, productRequest.Quantity))
        {
            return BadRequest("Invalid product code or quantity");
        }
        // get userId from token, parse to int and assign via "out"
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid token");
        }
        // catch service exceptions and return error to users
        try
        {
            var result = await _productsService.InventoryInsertProduct(productRequest.Code!, productRequest.Quantity, productRequest.SessionId, userId);
            if (result)
            {
                return Ok("Product inserted successfully.");
            }
            return StatusCode(500, "An error occurred while inserting the product.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    // Endpoint to get grouped products of a session
    [HttpGet("session/{sessionId}/grouped")]
    public async Task<IActionResult> GetSessionGroupedProducts(int sessionId, [FromQuery] SessionProductsFilterRequest filter)
    {
        var products = await _productsService.GetSessionGroupedProducts(sessionId, filter);
        return Ok(products);
    }
    // Endpoint to get product details by code and session
    [HttpGet("session/{sessionId}/details/{code}")]
    public async Task<IActionResult> GetProductsDetails(string code, int sessionId)
    {
        var products = await _productsService.GetProductsDetails(code, sessionId);
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