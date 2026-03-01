using Csinv.InventorySessions.DTOs;

namespace Csinv.InventorySessions.Interfaces;

public interface ISessionRepository
{
    Task<SessionResponse?> GetActiveSession();
    Task<SessionResponse> CreateSession(SessionStartRequest request);
    Task<bool> FinishSession(int sessionId);
    Task<List<SessionResponse>> GetAllSessions();
}