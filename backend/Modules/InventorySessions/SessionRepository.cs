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
            string cmdGetActiveSession = @"SELECT ses_id, ses_year, ses_status, ses_started_at, ses_finished_at 
            FROM cs_inventory_sessions 
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
                    Status = reader.GetString("ses_status"),
                    StartDate = reader.GetDateTime("ses_started_at"),
                    FinishDate = reader.IsDBNull(reader.GetOrdinal("ses_finished_at")) ? null : reader.GetDateTime("ses_finished_at")
                };
            }
            return null;
        } catch (Exception ex)
        {
            Console.WriteLine($"Error getting active session: {ex.Message}");
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
            string cmdStartSession = @"INSERT INTO cs_inventory_sessions (ses_year) VALUES (@ses_year)";
            using MySqlCommand startSessioncommand = new MySqlCommand(cmdStartSession, connection);
            startSessioncommand.Parameters.AddWithValue("@ses_year", request.Year);
            await startSessioncommand.ExecuteNonQueryAsync();

            // Get the ID of the newly created session (mysql last inserted id)
            int newSessionId = (int)startSessioncommand.LastInsertedId;
            
            // get to return the created session details
            string cmdGetSession = @"SELECT ses_id, ses_year, ses_status, ses_started_at, ses_finished_at 
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
                    Status = reader.GetString("ses_status"),
                    StartDate = reader.GetDateTime("ses_started_at"),
                    FinishDate = reader.IsDBNull(reader.GetOrdinal("ses_finished_at")) ? null : reader.GetDateTime("ses_finished_at")
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
    // Method to get all inventory sessions
    public async Task<List<SessionResponse>> GetAllSessions()
    {
        try
        {
            using MySqlConnection connection = DatabaseConnection.Connection();
            await connection.OpenAsync();
            string cmdGetAllSessions = @"SELECT ses_id, ses_year, ses_status, ses_started_at, ses_finished_at
            FROM cs_inventory_sessions
            ORDER BY ses_started_at DESC";
            using MySqlCommand getAllSessionscommand = new MySqlCommand(cmdGetAllSessions, connection);
            using var reader = (MySqlDataReader)await getAllSessionscommand.ExecuteReaderAsync();
            List<SessionResponse> sessions = new List<SessionResponse>();
            while (await reader.ReadAsync())
            {
                sessions.Add(new SessionResponse{
                    Id = reader.GetInt32("ses_id"),
                    Year = reader.GetInt32("ses_year"),
                    Status = reader.GetString("ses_status"),
                    StartDate = reader.GetDateTime("ses_started_at"),
                    FinishDate = reader.IsDBNull(reader.GetOrdinal("ses_finished_at")) ? null : reader.GetDateTime("ses_finished_at")
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