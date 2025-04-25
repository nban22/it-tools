using it_tools.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace it_tools.Data.Repositories;

public class FavouriteToolRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<FavouriteToolRepository> logger) : IFavouriteToolRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
    private readonly ILogger<FavouriteToolRepository> _logger = logger;

    public async Task AddAsync(Guid userId, Guid toolId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var favorite = new FavouriteTool
            {
                Id = Guid.NewGuid(),
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

    public async Task<List<Tool?>> GetAllByUserIdAsync(Guid userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var tools = await context.FavouriteTools
                .Where(f => f.UserId == userId)
                .Include(f => f.Tool)
                .Select(f => f.Tool)
                .ToListAsync();
                
            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorite tools for user {UserId}", userId);
            throw;
        }
    }

    public async Task RemoveAsync(Guid userId, Guid toolId)
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
    
    public async Task<bool> ExistsAsync(Guid userId, Guid toolId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.FavouriteTools
                .AnyAsync(f => f.UserId == userId && f.ToolId == toolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if favorite tool {ToolId} exists for user {UserId}", toolId, userId);
            throw;
        }
    }
}