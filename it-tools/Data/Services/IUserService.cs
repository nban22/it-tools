using it_tools.Data.Models;
using it_tools.Data.DTOs;

namespace it_tools.Data.Services;

public interface IUserService
{
    Task<bool> RegisterUserAsync(string email, string password, string? userName = null);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<AuthenticationResult> AuthenticateUserAsync(string email, string password);
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<bool> SetUserPremiumStatusAsync(string userId, bool isPremium);
    Task<bool> RequestPremiumStatusAsync(string userId);
}