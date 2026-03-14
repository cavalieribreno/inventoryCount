using Csinv.InventorySessions.DTOs;

namespace Csinv.InventorySessions.Interfaces;

public interface ISessionRepository
{
    Task<SessionResponse?> GetActiveSession();
    Task<bool> SessionExistsByYearMonth(int year, int? month);
    Task<SessionResponse> CreateSession(SessionStartRequest request, int userId);
    Task<bool> FinishSession(int sessionId, int userId);
    Task<bool> CancelSession(int sessionId, int userId);
    Task<List<SessionResponse>> GetAllSessions(SessionFilterRequest filter);
}