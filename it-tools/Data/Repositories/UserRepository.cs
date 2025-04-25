using Microsoft.EntityFrameworkCore;
using it_tools.Data.Models;

namespace it_tools.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<UserRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<User?> GetUserEntityByIdAsync(Guid userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.FavouriteTools!)
                .ThenInclude(ft => ft.Tool)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user entity with ID {UserId}", userId);
            throw;
        }
    }

    public async Task<User?> GetUserEntityByEmailAsync(string email)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user entity with email {Email}", email);
            throw;
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with email {Email}", user.Email);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            context.Users.Update(user);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user with ID {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> UpdateUserPropertyAsync(Guid userId, string propertyName, object value)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            var property = typeof(User).GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property {propertyName} not found on User entity", nameof(propertyName));
            }

            property.SetValue(user, value);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update {PropertyName} for user {UserId}", propertyName, userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            context.Users.Remove(user);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user with ID {UserId}", userId);
            throw;
        }
    }
}