// UserService.cs

using it_tools.Data;

// using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace it_tools.Services;

public class UserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public UserService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<bool> RegisterUserAsync(string email, string password)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
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

        _dbContext.Users.Add(user);
        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email) ?? null;
    }

    public async Task<AuthenticationResult> AuthenticateUserAsync(string email, string password)
    {
        // Lấy người dùng từ cơ sở dữ liệu bằng email
        var user = await GetUserByEmailAsync(email);
        if (user == null)
        {
            return AuthenticationResult.Failure("Email hoặc mật khẩu không đúng.");
        }

        // Xác minh mật khẩu
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return AuthenticationResult.Failure("Email hoặc mật khẩu không đúng.");
        }

        // Hiện tại không có xác thực hai yếu tố hoặc khóa tài khoản, nên trả về thành công
        return AuthenticationResult.Success();
    }

}