using MySql.Data.MySqlClient;
using Csinv.Database;
using Csinv.Products.Interfaces;
using Csinv.Products.DTOs;

namespace Csinv.Products.Repository;

// Repository class for product operations
public class ProductsRepository : IProductsRepository
{
    // Method to insert a product into the database
    public async Task<bool> InsertProduct(string productCode, int productQuantity, int year)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdInsertProduct = @"INSERT INTO cs_inventory (pro_code, inv_quantity, inv_year) VALUES (@productCode, @productQuantity, @year)";
            using MySqlCommand command = new MySqlCommand(cmdInsertProduct, connection);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productQuantity", productQuantity);
            command.Parameters.AddWithValue("@year", year);
            await command.ExecuteNonQueryAsync();

            return true;

        } catch (Exception ex)
        {
            Console.WriteLine($"Error inserting product: {ex.Message}");
            return false;
        }
    }
    // Method to get products based on filter
    public async Task<List<ProductsFilterResponse>> GetProductsByFilter(ProductsFilterRequest filter)
    {
        // list to hold the resulting products
        List<ProductsFilterResponse> products = new List<ProductsFilterResponse>();
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            // Prepare the command with dynamic query
            using MySqlCommand command = new MySqlCommand();
            // Base SQL query
            string cmdSelectProducts = @"SELECT inv_year, pro_code, pro_name, total_quantity FROM vw_inventory_items WHERE 1=1";
        
            // Dinamic query construction based on filter
            if (!string.IsNullOrEmpty(filter.ProductName))
            {
                cmdSelectProducts += " AND pro_name LIKE @productName";
                command.Parameters.AddWithValue("@productName", $"%{filter.ProductName}%");
            }
            if (!string.IsNullOrEmpty(filter.Code))
            {
                cmdSelectProducts += " AND pro_code = @code";
                command.Parameters.AddWithValue("@code", filter.Code);
            }
            if(filter.Year.HasValue)
            {
                cmdSelectProducts += " AND inv_year = @year";
                command.Parameters.AddWithValue("@year", filter.Year.Value);
            }
            // Finalize command setup
            command.CommandText = cmdSelectProducts;
            command.Connection = connection;

            // Add pagination parameters
            int offset = (filter.Page - 1) * filter.PageSize;
            command.CommandText += " ORDER BY pro_name LIMIT @pageSize OFFSET @offset";
            command.Parameters.AddWithValue("@pageSize", filter.PageSize);
            command.Parameters.AddWithValue("@offset", offset);


            // Execute the command and read results
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new ProductsFilterResponse
                {
                    ProductName = reader["pro_name"].ToString(),
                    Code = reader["pro_code"].ToString(),
                    Year = Convert.ToInt32(reader["inv_year"]),
                    TotalQuantity = Convert.ToInt32(reader["total_quantity"])
                });
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving products: {ex.Message}");
        }
        return products;
    }
    // Method to get all products details by code  
    public async Task<List<ProductsDetailsResponse>> GetProductsDetailsByCode(string code)
    {
        List<ProductsDetailsResponse> products = new List<ProductsDetailsResponse>();
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdSelectProducts = @"SELECT inv_id, pro_code, inv_quantity, inv_year, inv_date_added FROM cs_inventory WHERE pro_code = @code";
            using MySqlCommand command = new MySqlCommand(cmdSelectProducts, connection);
            command.Parameters.AddWithValue("@code", code);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new ProductsDetailsResponse
                {
                    Id = Convert.ToInt32(reader["inv_id"]),
                    Code = reader["pro_code"].ToString(),
                    Quantity = Convert.ToInt32(reader["inv_quantity"]),
                    Year = Convert.ToInt32(reader["inv_year"]),
                    DateHour = Convert.ToDateTime(reader["inv_date_added"])
                });
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving product details: {ex.Message}");
        }
        return products;
    }
    // 
    public async Task<bool> DeleteProductById(int productId)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdDeleteProductById = @"DELETE FROM cs_inventory WHERE inv_id = @productId";
            using MySqlCommand command = new MySqlCommand(cmdDeleteProductById, connection);
            command.Parameters.AddWithValue("@productId", productId);
            await command.ExecuteNonQueryAsync();
            return true;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error to delete product: {ex.Message}");
            return false;
        }
    }
}