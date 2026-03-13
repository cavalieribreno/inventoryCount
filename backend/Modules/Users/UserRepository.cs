using Csinv.Database;
using Csinv.Users.DTOs;
using Csinv.Users.Interfaces;
using MySql.Data.MySqlClient;

namespace Csinv.Users.Repository;
// Repository for user-related database operations
public class UserRepository : IUserRepository
{
    // Method to register a new user in the database
    public async Task<bool> RegisterUser(UserRegisterRequest request)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdRegisterUser = @"INSERT INTO cs_users (usr_name, usr_email, usr_password_hash) VALUES (@name, @email, @password)";
            using MySqlCommand command = new MySqlCommand(cmdRegisterUser, connection);
            command.Parameters.AddWithValue("@name", request.Name);
            command.Parameters.AddWithValue("@email", request.Email);
            command.Parameters.AddWithValue("@password", request.Password);
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error registering user: {ex.Message}");
            throw;
        }
    }
    // Method to authenticate a user and return a login response
    public async Task<LoginResponse?> LoginUser(UserLoginRequest request)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdLoginUser = @"SELECT usr_id, usr_name, usr_email, usr_password_hash FROM cs_users WHERE usr_email = @email";
            using MySqlCommand command = new MySqlCommand(cmdLoginUser, connection);
            command.Parameters.AddWithValue("@email", request.Email);
            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string passHash = reader.GetString("usr_password_hash");
                if (BCrypt.Net.BCrypt.Verify(request.Password, passHash))
                {
                    return new LoginResponse
                    {
                        UserId = reader.GetInt32("usr_id"),
                        Name = reader.GetString("usr_name"),
                        Email = reader.GetString("usr_email")
                    };
                }
            }
        } catch (Exception ex)
        {
            Console.WriteLine($"Error logging in user: {ex.Message}");
            throw;
        }
        return null;
    }

}