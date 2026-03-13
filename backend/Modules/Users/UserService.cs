using Csinv.Users.DTOs;
using Csinv.Users.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Csinv.Users.Services;
// Service for user-related business logic
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    // Method to register a new user
     public async Task<bool> RegisterUser(UserRegisterRequest request)
    {
        request.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        return await _userRepository.RegisterUser(request);
    }
    // Method to authenticate a user and return a login response
    public async Task<LoginResponse?> LoginUser(UserLoginRequest request)
    {
        var user = await _userRepository.LoginUser(request);
        if(user == null) return null;
        
        // Generate JWT token
        user.Token = GenerateJwtToken(user);
        return user;
    }
    // Helper method to generate JWT token for authenticated user
    private string GenerateJwtToken(LoginResponse user)
    {
        // Load JWT settings from environment variables
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;

        // Create security key and signing credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define claims for the token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name!),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}