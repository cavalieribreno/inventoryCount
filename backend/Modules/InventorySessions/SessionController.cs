using Csinv.InventorySessions.DTOs;
using Csinv.InventorySessions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Csinv.InventorySessions.Controller;
[ApiController]
[Route("api/sessions")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }
    // Endpoint to get the active inventory session
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSession()
    {
        var activeSession = await _sessionService.GetActiveSession();
        if(activeSession == null)
        {
            return NotFound("No active session found.");
        }
        return Ok(activeSession);
    }
    // Endpoint to start a new inventory session
    [HttpPost("create")]
    public async Task<IActionResult> CreateSession([FromBody]SessionStartRequest request)
    {
        var result = await _sessionService.CreateSession(request);
        if(result == null)
        {
            return BadRequest("Failed to create session. Please check the input data.");
        }
        return Ok(result);
    }
    // Endpoint to finish the active inventory session
    [HttpPatch("{sessionId}/finish")]
    public async Task<IActionResult> FinishSession(int sessionId)
    {
        var result = await _sessionService.FinishSession(sessionId);
        if(!result)
        {
            return BadRequest("Failed to finish session. No active session found.");
        }
        return Ok(result);
    }
    // Endpoint to get all inventory sessions
    [HttpGet("getall")]
    public async Task<IActionResult> GetAllSessions()
    {
        var sessions = await _sessionService.GetAllSessions();
        return Ok(sessions);
    }
}