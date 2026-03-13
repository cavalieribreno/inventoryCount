namespace Csinv.Users.DTOs;
// DTO for user registration request
public class UserRegisterRequest
{
    public string? Email {get; set;}
    public string? Name {get; set;}
    public string? Password {get; set;}
}
// DTO for user login request
public class UserLoginRequest
{
    public string? Email {get; set;}
    public string? Password {get; set;}
}
// DTO for user login response
public class LoginResponse
{
    public string? Token {get; set;}
    public int UserId {get; set;}
    public string? Name {get; set;}
    public string? Email {get; set;}
}