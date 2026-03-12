using MySql.Data.MySqlClient;
using Csinv.Database;
using Csinv.InventoryProducts.Interfaces;
using Csinv.InventoryProducts.DTOs;

namespace Csinv.InventoryProducts.Repository;

// Repository class for product operations
public class InventoryProductsRepository : IInventoryProductsRepository
{
    // Method to insert a product into the database inventory table
    public async Task<bool> InventoryInsertProduct(string productCode, int productQuantity, int sessionId)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdInsertProduct = @"INSERT INTO cs_inventory_items (pro_code, inv_quantity, ses_id) VALUES (@productCode, @productQuantity, @sessionId)";
            using MySqlCommand command = new MySqlCommand(cmdInsertProduct, connection);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productQuantity", productQuantity);
            command.Parameters.AddWithValue("@sessionId", sessionId);
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
            string cmdSelectProducts = @"SELECT ses_year, ses_month, pro_code, pro_name, total_quantity FROM vw_inventory_items WHERE 1=1";
        
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
                cmdSelectProducts += " AND ses_year = @year";
                command.Parameters.AddWithValue("@year", filter.Year.Value);
            }
            if(filter.Month.HasValue)
            {
                cmdSelectProducts += " AND ses_month = @month";
                command.Parameters.AddWithValue("@month", filter.Month.Value);
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
                    Year = Convert.ToInt32(reader["ses_year"]),
                    Month = reader["ses_month"] == DBNull.Value ? null : Convert.ToInt32(reader["ses_month"]),
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
            string cmdSelectProducts = @"SELECT i.inv_id, i.pro_code, p.pro_name, i.inv_quantity, s.ses_year, s.ses_month, i.inv_date_added
            FROM cs_inventory_items i
            INNER JOIN cs_inventory_sessions s ON i.ses_id = s.ses_id
            INNER JOIN cs_products p ON i.pro_code = p.pro_code
            WHERE i.pro_code = @code";
            using MySqlCommand command = new MySqlCommand(cmdSelectProducts, connection);
            command.Parameters.AddWithValue("@code", code);
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
                    DateHour = Convert.ToDateTime(reader["inv_date_added"])
                });
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving product details: {ex.Message}");
        }
        return products;
    }
    // Method to get all products by sessionId
    public async Task<List<ProductsDetailsResponse>> GetSessionProducts(int sessionId)
    {
        List<ProductsDetailsResponse> products = new List<ProductsDetailsResponse>();
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdSessionProducts = @"SELECT i.inv_id, i.pro_code, p.pro_name, i.inv_quantity, s.ses_year, s.ses_month, i.inv_date_added
            FROM cs_inventory_items i
            INNER JOIN cs_inventory_sessions s ON i.ses_id = s.ses_id
            INNER JOIN cs_products p ON i.pro_code = p.pro_code
            WHERE i.ses_id = @sessionId
            ORDER BY i.inv_date_added DESC";
            using MySqlCommand command = new MySqlCommand(cmdSessionProducts, connection);
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
                    DateHour = Convert.ToDateTime(reader["inv_date_added"])
                });
            }
            return products;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving session products: {ex.Message}");
            return products;
        }
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