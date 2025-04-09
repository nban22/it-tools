using Microsoft.EntityFrameworkCore;
using it_tools.Data.Models;

namespace it_tools.Data.Services;

public class UserService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IConfiguration configuration) : IUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory = dbContextFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<bool> RegisterUserAsync(string email, string password)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            return false;
        }
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            PasswordHash = hashedPassword,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow,
            PremiumRequest = false,
        };

        dbContext.Users.Add(user);
        var result = await dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email) ?? null;
    }

    public async Task<AuthenticationResult> AuthenticateUserAsync(string email, string password)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return AuthenticationResult.Failure("Email hoặc mật khẩu không đúng.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return AuthenticationResult.Failure("Email hoặc mật khẩu không đúng.");
        }

        return AuthenticationResult.Success();
    }
}