using System.Security.Claims;
using Csinv.InventorySessions.DTOs;
using Csinv.InventorySessions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Csinv.InventorySessions.Controller;
[ApiController]
[Route("api/sessions")]
[Authorize]
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
    // Endpoint to get a session by id
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSessionById(int sessionId)
    {
        var session = await _sessionService.GetSessionById(sessionId);
        if(session == null)
        {
            return NotFound("Session not found.");
        }
        return Ok(session);
    }
    // Endpoint to start a new inventory session
    [HttpPost("create")]
    public async Task<IActionResult> CreateSession([FromBody]SessionStartRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _sessionService.CreateSession(request, userId);
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
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _sessionService.FinishSession(sessionId, userId);
        if(!result)
        {
            return BadRequest("Failed to finish session. No active session found.");
        }
        return Ok(result);
    }
    // Endpoint to cancel the active inventory session
    [HttpPatch("{sessionId}/cancel")]
    public async Task<IActionResult> CancelSession(int sessionId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _sessionService.CancelSession(sessionId, userId);
        if(!result)
        {
            return BadRequest("Failed to cancel session. No active session found.");
        }
        return Ok(result);
    }
    // Endpoint to get all inventory sessions with filters and pagination
    [HttpGet("getall")]
    public async Task<IActionResult> GetAllSessions([FromQuery] SessionFilterRequest filter)
    {
        var sessions = await _sessionService.GetAllSessions(filter);
        return Ok(sessions);
    }
}