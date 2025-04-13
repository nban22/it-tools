// Data/Services/AdminService.cs
using Microsoft.EntityFrameworkCore;
using it_tools.Data.Models;

namespace it_tools.Data.Services;

public class AdminService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IAdminService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory = dbContextFactory;

    public async Task<Admin?> GetAdminByEmailAsync(string email)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Admins.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<AuthenticationResult> AuthenticateAdminAsync(string email, string password)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var admin = await dbContext.Admins.FirstOrDefaultAsync(a => a.Email == email);
        if (admin == null)
        {
            return AuthenticationResult.Failure("Invalid email or password.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
        if (!isPasswordValid)
        {
            return AuthenticationResult.Failure("Invalid email or password.");
        }

        return AuthenticationResult.Success();
    }
}