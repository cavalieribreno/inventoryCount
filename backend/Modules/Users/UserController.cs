using Csinv.Users.DTOs;
using Csinv.Users.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Csinv.Users.Controller;

// Controller for handling user-related API requests
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    // Endpoint for user registration
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequest request)
    {
        try
        {
            var success = await _userService.RegisterUser(request);
            if (success)
                return Ok(new { message = "User registered successfully" });
            else
                return BadRequest(new { message = "Failed to register user" });
        } catch (Exception ex)
        {
            Console.WriteLine($"Error in RegisterUser: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    // Endpoint for user login
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginRequest request)
    {
        try
        {
            var response = await _userService.LoginUser(request);
            if (response != null)
                return Ok(response);
            else
                return Unauthorized(new { message = "Invalid email or password" });
        } catch (Exception ex)
        {
            Console.WriteLine($"Error in LoginUser: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}