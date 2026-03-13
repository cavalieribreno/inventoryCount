using Csinv.Users.DTOs;

namespace Csinv.Users.Interfaces;
// Interface for user repository
public interface IUserRepository
{
    Task<bool> RegisterUser(UserRegisterRequest request);
    Task<LoginResponse?> LoginUser(UserLoginRequest request);
}