using Csinv.InventorySessions.DTOs;

namespace Csinv.InventorySessions.Interfaces;

public interface ISessionService
{
    Task<SessionResponse?> GetActiveSession();
    Task<SessionResponse?> GetSessionById(int sessionId);
    Task<SessionResponse> CreateSession(SessionStartRequest request, int userId);
    Task<bool> FinishSession(int sessionId, int userId);
    Task<bool> CancelSession(int sessionId, int userId);
    Task<List<SessionResponse>> GetAllSessions(SessionFilterRequest filter);
}