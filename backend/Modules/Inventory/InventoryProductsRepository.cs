using MySql.Data.MySqlClient;
using Csinv.Database;
using Csinv.InventoryProducts.Interfaces;
using Csinv.InventoryProducts.DTOs;

namespace Csinv.InventoryProducts.Repository;

// Repository class for product operations
public class InventoryProductsRepository : IInventoryProductsRepository
{
    // Method to insert a product into the database inventory table
    public async Task<bool> InventoryInsertProduct(string productCode, int productQuantity, int sessionId, int userId)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdInsertProduct = @"INSERT INTO cs_inventory_items (pro_code, inv_quantity, ses_id, usr_id) VALUES (@productCode, @productQuantity, @sessionId, @userId)";
            using MySqlCommand command = new MySqlCommand(cmdInsertProduct, connection);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productQuantity", productQuantity);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.Parameters.AddWithValue("@userId", userId);
            await command.ExecuteNonQueryAsync();

            return true;

        } catch (Exception ex)
        {
            Console.WriteLine($"Error inserting product: {ex.Message}");
            return false;
        }
    }
    // Method to check if a product exists by code
    public async Task<bool> ProductExistsByCode(string code)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdProductExists = @"SELECT COUNT(*) FROM cs_products WHERE pro_code = @code";
            using MySqlCommand command = new MySqlCommand(cmdProductExists, connection);
            command.Parameters.AddWithValue("@code", code);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error checking product by code: {ex.Message}");
            throw;
        }
    }
    // Method to get products grouped
    public async Task<List<SessionGroupedProductsResponse>> GetSessionGroupedProducts(int sessionId, SessionProductsFilterRequest filter)
    {
        List<SessionGroupedProductsResponse> products = new List<SessionGroupedProductsResponse>();
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            using MySqlCommand command = new MySqlCommand();
            string cmdGroupedProducts = @"SELECT i.pro_code, p.pro_name, SUM(i.inv_quantity) AS total_quantity
            FROM cs_inventory_items i
            INNER JOIN cs_products p ON i.pro_code = p.pro_code
            WHERE i.ses_id = @sessionId";
            command.Parameters.AddWithValue("@sessionId", sessionId);

            // Dynamic query construction based on filter
            if (!string.IsNullOrEmpty(filter.ProductName))
            {
                cmdGroupedProducts += " AND p.pro_name LIKE @productName";
                command.Parameters.AddWithValue("@productName", $"%{filter.ProductName}%");
            }
            if (!string.IsNullOrEmpty(filter.Code))
            {
                cmdGroupedProducts += " AND i.pro_code = @code";
                command.Parameters.AddWithValue("@code", filter.Code);
            }
            int offset = (filter.Page - 1) * filter.PageSize;

            // Group by product code, order by name and apply pagination
            cmdGroupedProducts += " GROUP BY i.pro_code, p.pro_name ORDER BY p.pro_name LIMIT @pageSize OFFSET @offset";
            command.Parameters.AddWithValue("@pageSize", filter.PageSize);
            command.Parameters.AddWithValue("@offset", offset);
            command.CommandText = cmdGroupedProducts;
            command.Connection = connection;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new SessionGroupedProductsResponse
                {
                    Code = reader["pro_code"].ToString(),
                    ProductName = reader["pro_name"].ToString(),
                    TotalQuantity = Convert.ToInt32(reader["total_quantity"])
                });
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving grouped products: {ex.Message}");
        }
        return products;
    }
    // Method to get all products details by code  
    public async Task<List<ProductsDetailsResponse>> GetProductsDetails(string code, int sessionId)
    {
        List<ProductsDetailsResponse> products = new List<ProductsDetailsResponse>();
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdSelectProducts = @"SELECT i.inv_id, i.pro_code, p.pro_name, i.inv_quantity, s.ses_year, s.ses_month, i.inv_date_added, u.usr_name
            FROM cs_inventory_items i
            INNER JOIN cs_inventory_sessions s ON i.ses_id = s.ses_id
            INNER JOIN cs_products p ON i.pro_code = p.pro_code
            INNER JOIN cs_users u ON i.usr_id = u.usr_id
            WHERE i.pro_code = @code AND i.ses_id = @sessionId";
            using MySqlCommand command = new MySqlCommand(cmdSelectProducts, connection);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new ProductsDetailsResponse
                {
                    Id = Convert.ToInt32(reader["inv_id"]),
                    Code = reader["pro_code"].ToString(),
                    ProductName = reader["pro_name"].ToString(),
                    Quantity = Convert.ToInt32(reader["inv_quantity"]),
                    Year = Convert.ToInt32(reader["ses_year"]),
                    Month = reader["ses_month"] == DBNull.Value ? null : Convert.ToInt32(reader["ses_month"]),
                    DateHour = Convert.ToDateTime(reader["inv_date_added"]),
                    UserName = reader["usr_name"].ToString()
                });
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving product details: {ex.Message}");
        }
        return products;
    }
    // Method to delete a product by product id
    public async Task<bool> DeleteProductById(int productId)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdDeleteProductById = @"DELETE FROM cs_inventory_items WHERE inv_id = @productId";
            using MySqlCommand command = new MySqlCommand(cmdDeleteProductById, connection);
            command.Parameters.AddWithValue("@productId", productId);
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0; 
        } catch (Exception ex)
        {
            Console.WriteLine($"Error to delete product: {ex.Message}");
            return false;
        }
    }
}