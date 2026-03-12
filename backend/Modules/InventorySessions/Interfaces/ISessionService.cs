using Csinv.InventorySessions.DTOs;

namespace Csinv.InventorySessions.Interfaces;

public interface ISessionService
{
    Task<SessionResponse?> GetActiveSession();
    Task<SessionResponse> CreateSession(SessionStartRequest request);
    Task<bool> FinishSession(int sessionId);
    Task<bool> CancelSession(int sessionId);
    Task<List<SessionResponse>> GetAllSessions(SessionFilterRequest filter);
}