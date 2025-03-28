using Microsoft.EntityFrameworkCore;
using it_tools.Data;
using it_tools.Repositories;
using Microsoft.Extensions.Logging;

public class UserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddFavoriteTool(string userId, string toolId)
    {
        try
        {
            var favorite = new FavouriteTool 
            { 
                Id = Guid.NewGuid().ToString(),
                UserId = userId, 
                ToolId = toolId 
            };
            
            _logger.LogInformation("Adding favorite tool {ToolId} for user {UserId}", toolId, userId);
            _context.FavouriteTools.Add(favorite);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully added favorite tool {ToolId} for user {UserId}", toolId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add favorite tool {ToolId} for user {UserId}", toolId, userId);
            throw; // Re-throw để component gọi có thể xử lý lỗi nếu cần
        }
    }

    public async Task RemoveFavoriteTool(string userId, string toolId)
    {
        try
        {
            _logger.LogInformation("Attempting to remove favorite tool {ToolId} for user {UserId}", toolId, userId);
            var favorite = await _context.FavouriteTools
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ToolId == toolId);
            
            if (favorite != null)
            {
                _context.FavouriteTools.Remove(favorite);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully removed favorite tool {ToolId} for user {UserId}", toolId, userId);
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
            _logger.LogInformation("Retrieving favorite tools for user {UserId}", userId);
            var favoriteTools = await _context.FavouriteTools
                .Where(f => f.UserId == userId)
                .Include(f => f.Tool)
                .Select(f => new ToolDto 
                { 
                    Id = f.Tool != null ? f.Tool.Id : string.Empty,
                    Name = f.Tool.Name,
                    Description = f.Tool.Description 
                })
                .ToListAsync();
            
            _logger.LogInformation("Successfully retrieved {Count} favorite tools for user {UserId}", 
                favoriteTools.Count, userId);
            return favoriteTools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorite tools for user {UserId}", userId);
            throw;
        }
    }
}