using Csinv.Database;
using Csinv.InventorySessions.DTOs;
using Csinv.InventorySessions.Interfaces;
using MySql.Data.MySqlClient;

namespace Csinv.InventorySessions.Repository;
// Repository class for inventory session operations
public class SessionRepository : ISessionRepository
{
    // Method to get the active inventory session
    public async Task<SessionResponse?> GetActiveSession()
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdGetActiveSession = @"SELECT ses_id, ses_year, ses_month, ses_status, ses_started_at, ses_finished_at, ses_canceled_at, totalqnt_items
            FROM vw_inventory_sessions 
            WHERE ses_status = 'active' 
            LIMIT 1";
            using MySqlCommand command = new MySqlCommand(cmdGetActiveSession, connection);
            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new SessionResponse
                {
                    Id = reader.GetInt32("ses_id"),
                    Year = reader.GetInt32("ses_year"),
                    Month = reader.IsDBNull(reader.GetOrdinal("ses_month")) ? null : reader.GetInt32("ses_month"),
                    Status = reader.GetString("ses_status"),
                    StartDate = reader.GetDateTime("ses_started_at"),
                    FinishDate = reader.IsDBNull(reader.GetOrdinal("ses_finished_at")) ? null : reader.GetDateTime("ses_finished_at"),
                    CancelDate = reader.IsDBNull(reader.GetOrdinal("ses_canceled_at")) ? null : reader.GetDateTime("ses_canceled_at"),
                    TotalItems = reader.GetInt32("totalqnt_items")
                };
            }
            return null;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error getting active session: {ex.Message}");
            throw;
        }
    }
    // Method to check if a session exists for the given year and month (not canceled)
    public async Task<bool> SessionExistsByYearMonth(int year, int? month)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdExistSession = @"SELECT COUNT(*) FROM cs_inventory_sessions
            WHERE ses_year = @year AND ses_status != 'canceled'";
            if(month.HasValue)
            {
                cmdExistSession += " AND ses_month = @month";
            }
            using MySqlCommand command = new MySqlCommand(cmdExistSession, connection);
            command.Parameters.AddWithValue("@year", year);
            if(month.HasValue)
            {
                command.Parameters.AddWithValue("@month", month.Value);
            }
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error checking session by year/month: {ex.Message}");
            throw;
        }
    }
    // Method to start a new inventory session
    public async Task<SessionResponse> CreateSession(SessionStartRequest request)
    {
        try
        {
            // create a new session
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdStartSession = @"INSERT INTO cs_inventory_sessions (ses_year, ses_month) VALUES (@ses_year, @ses_month)";
            using MySqlCommand startSessioncommand = new MySqlCommand(cmdStartSession, connection);
            startSessioncommand.Parameters.AddWithValue("@ses_year", request.Year);
            
            // if month is null, set parameter to DBNull.Value to insert NULL in the database
            startSessioncommand.Parameters.AddWithValue("@ses_month", request.Month.HasValue ? request.Month.Value : DBNull.Value);
            await startSessioncommand.ExecuteNonQueryAsync();

            // Get the ID of the newly created session (mysql last inserted id)
            int newSessionId = (int)startSessioncommand.LastInsertedId;
            
            // get to return the created session details
            string cmdGetSession = @"SELECT ses_id, ses_year, ses_month, ses_status, ses_started_at, ses_finished_at, ses_canceled_at
            FROM cs_inventory_sessions 
            WHERE ses_id = @ses_id";
            using MySqlCommand getSessionCommand = new MySqlCommand(cmdGetSession, connection);
            getSessionCommand.Parameters.AddWithValue("@ses_id", newSessionId);
            using var reader = (MySqlDataReader)await getSessionCommand.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SessionResponse
                {
                    Id = reader.GetInt32("ses_id"),
                    Year = reader.GetInt32("ses_year"),
                    Month = reader.IsDBNull(reader.GetOrdinal("ses_month")) ? null : reader.GetInt32("ses_month"),
                    Status = reader.GetString("ses_status"),
                    StartDate = reader.GetDateTime("ses_started_at"),
                    FinishDate = reader.IsDBNull(reader.GetOrdinal("ses_finished_at")) ? null : reader.GetDateTime("ses_finished_at"),
                    CancelDate = reader.IsDBNull(reader.GetOrdinal("ses_canceled_at")) ? null : reader.GetDateTime("ses_canceled_at")
                };
            }
            return null;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error creating session: {ex.Message}");
            throw;
        }
    }
    // Method to finish an active inventory session
    public async Task<bool> FinishSession(int sessionId)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdFinishSession = @"UPDATE cs_inventory_sessions 
            SET ses_status = 'finished', ses_finished_at = NOW()
            WHERE ses_id = @ses_id AND ses_status = 'active'";
            using MySqlCommand finishSessionCommand = new MySqlCommand(cmdFinishSession, connection);
            finishSessionCommand.Parameters.AddWithValue("@ses_id", sessionId);
            int rowsAffected = await finishSessionCommand.ExecuteNonQueryAsync();
            return rowsAffected > 0; // Return true if a session was updated (rowsAffected), else return false
        } catch (Exception ex)
        {
            Console.WriteLine($"Error finishing session: {ex.Message}");
            throw;
        }
    }
    // Method to cancel an active inventory session
    public async Task<bool> CancelSession(int sessionId)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdCancelSession = @"UPDATE cs_inventory_sessions 
            SET ses_status = 'canceled', ses_canceled_at = NOW()
            WHERE ses_id = @ses_id AND ses_status = 'active'";
            using MySqlCommand cancelSessionCommand = new MySqlCommand(cmdCancelSession, connection);
            cancelSessionCommand.Parameters.AddWithValue("@ses_id", sessionId);
            int rowsAffected = await cancelSessionCommand.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error canceling session: {ex.Message}");
            throw;
        }
    }
    // Method to get all inventory sessions with filters and pagination
    public async Task<List<SessionResponse>> GetAllSessions(SessionFilterRequest filter)
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            using MySqlCommand command = new MySqlCommand();
            string cmdGetAllSessions = @"SELECT ses_id, ses_year, ses_month, ses_status, ses_started_at, ses_finished_at, ses_canceled_at, totalqnt_items
            FROM vw_inventory_sessions WHERE 1=1";
            // Dynamic query construction based on filter
            if(filter.Year.HasValue)
            {
                cmdGetAllSessions += " AND ses_year = @year";
                command.Parameters.AddWithValue("@year", filter.Year.Value);
            }
            if(filter.Month.HasValue)
            {
                cmdGetAllSessions += " AND ses_month = @month";
                command.Parameters.AddWithValue("@month", filter.Month.Value);
            }
            if(!string.IsNullOrEmpty(filter.Status))
            {
                cmdGetAllSessions += " AND ses_status = @status";
                command.Parameters.AddWithValue("@status", filter.Status);
            }
            // Pagination
            int offset = (filter.Page - 1) * filter.PageSize;
            cmdGetAllSessions += " ORDER BY ses_started_at DESC LIMIT @pageSize OFFSET @offset";
            command.Parameters.AddWithValue("@pageSize", filter.PageSize);
            command.Parameters.AddWithValue("@offset", offset);
            command.CommandText = cmdGetAllSessions;
            command.Connection = connection;
            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            List<SessionResponse> sessions = new List<SessionResponse>();
            while (await reader.ReadAsync())
            {
                sessions.Add(new SessionResponse{
                    Id = reader.GetInt32("ses_id"),
                    Year = reader.GetInt32("ses_year"),
                    Month = reader.IsDBNull(reader.GetOrdinal("ses_month")) ? null : reader.GetInt32("ses_month"),
                    Status = reader.GetString("ses_status"),
                    StartDate = reader.GetDateTime("ses_started_at"),
                    FinishDate = reader.IsDBNull(reader.GetOrdinal("ses_finished_at")) ? null : reader.GetDateTime("ses_finished_at"),
                    CancelDate = reader.IsDBNull(reader.GetOrdinal("ses_canceled_at")) ? null : reader.GetDateTime("ses_canceled_at"),
                    TotalItems = reader.GetInt32("totalqnt_items")
                });
            }
            return sessions;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error getting all sessions: {ex.Message}");
            throw;
        }
    }
}