using Csinv.InventorySessions.DTOs;
using Csinv.InventorySessions.Interfaces;

namespace Csinv.InventorySessions.Service;
// Service class for inventory session operations
public class SessionService : ISessionService
{
    // Dependency injection of the session repository
    private readonly ISessionRepository _sessionRepository;
    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }
    // Method to get the active inventory session
    public async Task<SessionResponse?> GetActiveSession()
    {
        return await _sessionRepository.GetActiveSession();
    }
    // Method to start a new inventory session
    public async Task<SessionResponse> CreateSession(SessionStartRequest request)
    {
        // Validate input parameters
        if(request.Year < 2000 || request.Year > DateTime.Now.Year + 1)
        {
            throw new ArgumentException("Invalid year for inventory session.");
        }
        if(request.Month.HasValue && (request.Month < 1 || request.Month > 12))
        {
            throw new ArgumentException("Invalid month for inventory session.");
        }
        // Check for existing sessions in the same year and month
        if(await _sessionRepository.SessionExistsByYearMonth(request.Year, request.Month))
        {
            throw new InvalidOperationException("A session for the specified year and month already exists.");
        }
        // Check for an active session before creating a new one
        var activeSession = await _sessionRepository.GetActiveSession();
        if(activeSession != null)
        {
            throw new InvalidOperationException("An active inventory session already exists.");
        }
        return await _sessionRepository.CreateSession(request);
    }
    // Method to finish an inventory session
    public async Task<bool> FinishSession(int sessionId)
    {
        return await _sessionRepository.FinishSession(sessionId);
    }
    // Method to cancel an inventory session
    public async Task<bool> CancelSession(int sessionId)
    {
        return await _sessionRepository.CancelSession(sessionId);
    }
    // Method to get all inventory sessions
    public async Task<List<SessionResponse>> GetAllSessions()
    {
        return await _sessionRepository.GetAllSessions();
    }
}