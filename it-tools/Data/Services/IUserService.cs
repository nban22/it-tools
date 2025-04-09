
using it_tools.Data.Models;

namespace it_tools.Data.Services;

public interface IUserService 
{
    Task<bool> RegisterUserAsync(string email, string password);
    
    Task<User?> GetUserByEmailAsync(string email);
    
    Task<AuthenticationResult> AuthenticateUserAsync(string email, string password);
}