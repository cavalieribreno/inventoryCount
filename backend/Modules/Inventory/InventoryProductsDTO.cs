namespace Csinv.InventoryProducts.DTOs;
// Data Transfer Object for insert product requests
public class ProductsInsertRequest
{
    public string? Code { get; set; }
    public int Quantity { get; set; }
    public int SessionId { get; set; }
}
// Data Transfer Object for product details response
public class ProductsDetailsResponse
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int Quantity { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public DateTime DateHour { get; set;}
}
// Data Transfer Object to products filter requests and pagination
public class ProductsFilterRequest
{
    public string? ProductName { get; set; }
    public string? Code { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }

    // Pagination properties
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
// Data Transfer Object for product filter responses
public class ProductsFilterResponse
{
    public string? ProductName { get; set; }
    public string? Code { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int TotalQuantity { get; set; }
}