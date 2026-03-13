using Csinv.Users.DTOs;

namespace Csinv.Users.Interfaces;
// Interface for user service
public interface IUserService
{
    Task<bool> RegisterUser(UserRegisterRequest request);
    Task<LoginResponse?> LoginUser(UserLoginRequest request);
}