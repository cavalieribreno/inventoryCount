using MySql.Data.MySqlClient;

namespace Csinv.Database;
// Provides a method to create and return a MySQL database connection
public class DatabaseConnection
{
    public static MySqlConnection Connection()
    {
        string dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        string dbUser = Environment.GetEnvironmentVariable("DB_USER");
        string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        string dbName = Environment.GetEnvironmentVariable("DB_NAME");
        string dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        string connectionString = $"Server={dbHost};Database={dbName};User={dbUser};Password={dbPassword};Port={dbPort};";
        return new MySqlConnection(connectionString);
    }
}