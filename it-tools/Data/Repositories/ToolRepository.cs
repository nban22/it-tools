using Microsoft.EntityFrameworkCore;
using it_tools.Data.DTOs;
using it_tools.Data;
using it_tools.Data.Models;

namespace it_tools.Data.Repositories;

public class ToolRepository : IToolRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ToolRepository> _logger;

    public ToolRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<ToolRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Tool?> GetToolEntityByIdAsync(Guid toolId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.Tools
                .Include(t => t.Group)
                .FirstOrDefaultAsync(t => t.Id == toolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tool entity with ID {ToolId}", toolId);
            throw;
        }
    }

    public async Task<Tool?> GetToolEntityBySlugAsync(string slug)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.Tools
                .Include(t => t.Group)
                .FirstOrDefaultAsync(t => t.Slug == slug);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tool entity with slug {Slug}", slug);
            throw;
        }
    }

    public async Task<List<Tool>> GetAllEnabledToolEntitiesAsync()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.Tools
                .Include(t => t.Group)
                .Where(t => t.IsEnabled)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all enabled tool entities");
            throw;
        }
    }

    public async Task<List<Tool>> GetAllToolEntitiesAsync()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.Tools
                .Include(t => t.Group)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all tool entities");
            throw;
        }
    }

    public async Task<List<ToolGroup>> GetAllToolGroupEntitiesAsync()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.ToolGroups
                .Include(tg => tg.Tools)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all tool group entities");
            throw;
        }
    }

    public async Task<List<ToolGroup>> GetAllEnabledToolGroupEntitiesAsync()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.ToolGroups
                .Include(tg => tg.Tools)
                .Where(tg => tg.Tools.Any(t => t.IsEnabled))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all tool group entities with enabled tools");
            throw;
        }
    }

    public async Task<List<Guid>> GetFavoriteToolIdsForUserAsync(Guid userId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.FavouriteTools
                .Where(ft => ft.UserId == userId && ft.Tool != null && ft.Tool.IsEnabled)
                .Select(ft => ft.ToolId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching favorite tool IDs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Tool>> GetFavoriteToolEntitiesForUserAsync(Guid userId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var favoriteToolIds = await dbContext.FavouriteTools
                .Where(f => f.UserId == userId)
                .Select(f => f.ToolId)
                .ToListAsync();

            return await dbContext.Tools
                .Include(t => t.Group)
                .Where(t => favoriteToolIds.Contains(t.Id) && t.IsEnabled)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching favorite tool entities for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ToolGroup?> GetToolGroupByNameAsync(string name)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            return await dbContext.ToolGroups
                .FirstOrDefaultAsync(tg => tg.Name == name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tool group with name {Name}", name);
            throw;
        }
    }

    public async Task<ToolGroup> CreateToolGroupAsync(ToolGroup toolGroup)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            dbContext.ToolGroups.Add(toolGroup);
            await dbContext.SaveChangesAsync();
            return toolGroup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tool group {Name}", toolGroup.Name);
            throw;
        }
    }

    public async Task<Tool> CreateToolAsync(Tool tool)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            dbContext.Tools.Add(tool);
            await dbContext.SaveChangesAsync();
            return tool;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tool {Name}", tool.Name);
            throw;
        }
    }

    public async Task UpdateToolAsync(Tool tool)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            dbContext.Tools.Update(tool);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tool with ID {ToolId}", tool.Id);
            throw;
        }
    }

    public async Task<bool> DeleteToolAsync(Guid toolId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            
            // Remove favorite references first
            var favoriteToolRecords = await dbContext.FavouriteTools
                .Where(ft => ft.ToolId == toolId)
                .ToListAsync();
                
            if (favoriteToolRecords.Any())
            {
                dbContext.FavouriteTools.RemoveRange(favoriteToolRecords);
                await dbContext.SaveChangesAsync();
            }

            // Then remove the tool
            var tool = await dbContext.Tools.FindAsync(toolId);
            if (tool == null)
            {
                return false;
            }

            var toolGroupId = tool.GroupId;
            dbContext.Tools.Remove(tool);
            await dbContext.SaveChangesAsync();

            // Check if group is empty and delete if needed
            var remainingToolsInGroup = await dbContext.Tools
                .Where(t => t.GroupId == toolGroupId)
                .AnyAsync();

            if (!remainingToolsInGroup)
            {
                var toolGroup = await dbContext.ToolGroups.FindAsync(toolGroupId);
                if (toolGroup != null)
                {
                    dbContext.ToolGroups.Remove(toolGroup);
                    await dbContext.SaveChangesAsync();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool with ID {ToolId}", toolId);
            throw;
        }
    }

    public async Task<bool> UpdateToolPropertyAsync(Guid toolId, string propertyName, object value)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools.FindAsync(toolId);
            if (tool == null)
            {
                return false;
            }

            var property = typeof(Tool).GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property {propertyName} not found on Tool entity", nameof(propertyName));
            }

            property.SetValue(tool, value);
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {PropertyName} for tool {ToolId}", propertyName, toolId);
            throw;
        }
    }
}