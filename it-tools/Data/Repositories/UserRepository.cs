using Microsoft.EntityFrameworkCore;
using it_tools.Data.DTOs;
using it_tools.Data.Models;

namespace it_tools.Data.Repositories;
public class UserRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<UserRepository> logger) : IUserRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
    private readonly ILogger<UserRepository> _logger = logger;
    public async Task AddFavoriteTool(string userId, string toolId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var favorite = new FavouriteTool
            { 
                Id = Guid.NewGuid().ToString(),
                UserId = userId, 
                ToolId = toolId 
            };
            context.FavouriteTools.Add(favorite);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add favorite tool {ToolId} for user {UserId}", toolId, userId);
            throw;
        }
    }

    public async Task RemoveFavoriteTool(string userId, string toolId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var favorite = await context.FavouriteTools
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ToolId == toolId);
            
            if (favorite != null)
            {
                context.FavouriteTools.Remove(favorite);
                await context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Favorite tool {ToolId} not found for user {UserId}", toolId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove favorite tool {ToolId} for user {UserId}", toolId, userId);
            throw;
        }
    }

    public async Task<List<ToolDto>> GetFavoriteTools(string userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            _logger.LogInformation("Retrieving favorite tools for user {UserId}", userId);
            var favoriteTools = await context.FavouriteTools
                .Where(f => f.UserId == userId)
                .Include(f => f.Tool)
                .Select(f => new ToolDto 
                { 
                    Id = f.Tool != null ? f.Tool.Id : string.Empty,
                    Name = f.Tool != null ? f.Tool.Name : string.Empty,
                    Description = f.Tool != null ? f.Tool.Description : string.Empty,
                })
                .ToListAsync();
            return favoriteTools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorite tools for user {UserId}", userId);
            throw;
        }
    }

    //Task<UserDto?> GetUserById(string userId);
    public async Task<UserDto?> GetUserById(string userId) {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users
                .Include(u => u.FavouriteTools!)
                .ThenInclude(ft => ft.Tool)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                PremiumRequest = user.PremiumRequest,
                FavoriteTools = user.FavouriteTools?.Select(ft => new ToolDto
                {
                    Id = ft.Tool != null ? ft.Tool.Id : string.Empty,
                    Name = ft.Tool != null ? ft.Tool.Name : string.Empty,
                    Description = ft.Tool != null ? ft.Tool.Description : string.Empty,
                }).ToList() ?? new List<ToolDto>()
            };

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user {UserId}", userId);
            throw;
        }
    }
}