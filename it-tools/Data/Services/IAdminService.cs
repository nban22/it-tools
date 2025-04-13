// Data/Services/IAdminService.cs
using it_tools.Data.Models;

namespace it_tools.Data.Services;

public interface IAdminService
{
    Task<AuthenticationResult> AuthenticateAdminAsync(string email, string password);
    Task<Admin?> GetAdminByEmailAsync(string email);
}