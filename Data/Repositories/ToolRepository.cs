
using Microsoft.EntityFrameworkCore;

using it_tools.Data.DTOs;
using it_tools.Data;
using it_tools.Data.Models;
using System.Runtime.CompilerServices;

namespace it_tools.Data.Repositories;

public class ToolRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<ToolRepository> logger) : IToolRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
    private readonly ILogger<ToolRepository> _logger = logger;

    // Lấy danh sách Tools cho người dùng chưa xác thực (unauthorized)
    public async Task<Tool?> GetToolByIdAsync(string toolId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools
                .Include(t => t.Group) // Include the ToolGroup if needed
                .FirstOrDefaultAsync(t => t.Id == toolId);

            return tool;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tool with ID {ToolId}", toolId);
            throw;
        }
    }
    public async Task<List<ToolDto>> GetAllToolsForUnauthorizedAsync()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tools = await dbContext.Tools
                .Where(t => t.IsEnabled) // Chỉ lấy các tool đang bật
                .Select(t => new ToolDto
                {
                    Id = t.Id,
                    GroupId = t.GroupId,
                    Name = t.Name,
                    Description = t.Description,
                    IsPremium = t.IsPremium,
                    Slug = t.Slug,
                    Icon = t.Icon,
                    IsEnabled = t.IsEnabled,
                    IsFavorite = false
                })
                .ToListAsync();

            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tools for unauthorized users.");
            throw;
        }
    }

    // Lấy danh sách Tools cho người dùng đã đăng nhập, bao gồm IsFavorite
    public async Task<List<ToolDto>> GetAllToolsForUserAsync(string userId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            // Lấy danh sách các ToolId yêu thích của người dùng
            var favoriteToolIds = await dbContext.FavouriteTools
                .Where(ft => ft.UserId == userId)
                .Select(ft => ft.ToolId)
                .ToListAsync();

            // Lấy danh sách Tools và kiểm tra IsFavorite
            var tools = await dbContext.Tools
                .Where(t => t.IsEnabled) // Chỉ lấy các tool đang bật
                .Select(t => new ToolDto
                {
                    Id = t.Id,
                    GroupId = t.GroupId,
                    Name = t.Name,
                    Description = t.Description,
                    IsPremium = t.IsPremium,
                    Slug = t.Slug,
                    Icon = t.Icon,
                    IsEnabled = t.IsEnabled,
                    IsFavorite = favoriteToolIds.Contains(t.Id) // Kiểm tra xem Tool có trong danh sách yêu thích không
                })
                .ToListAsync();
            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tools for user {UserId}", userId);
            throw;
        }
    }
    public async Task<List<ToolGroupDto>> GetAllToolGroups()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var allTools = await dbContext.ToolGroups
                .Include(tg => tg.Tools)
                .Where(tg => tg.Tools.Any(t => t.IsEnabled)) // Only include groups with at least one enabled tool
                .Select(tg => new ToolGroupDto
                {
                    Id = tg.Id,
                    Name = tg.Name,
                    Description = tg.Description,
                    IsExpanded = false,
                    Tools = tg.Tools
                        .Where(t => t.IsEnabled)
                        .Select(t => new ToolDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Description = t.Description,
                            IsPremium = t.IsPremium,
                            Slug = t.Slug,
                            Icon = t.Icon,
                            GroupId = t.GroupId,
                            IsEnabled = t.IsEnabled,
                            IsFavorite = false // Default to false, can be updated later if needed
                        }).ToList()
                })
                .ToListAsync();

           
            
            return allTools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all tool groups for user");
            throw;
        }
    }

    public async Task<ToolGroupDto> GetFavoriteToolGroup(string userId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var favoriteToolIds = await dbContext.FavouriteTools
                .Where(f => f.UserId == userId)
                .Select(f => f.ToolId)
                .ToListAsync();

            var favoriteTools = await dbContext.Tools
                .Where(t => favoriteToolIds.Contains(t.Id))
                .Select(t => new ToolDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    IsPremium = t.IsPremium,
                    Slug = t.Slug,
                    Icon = t.Icon,
                    IsEnabled = t.IsEnabled,
                    IsFavorite = true // All tools in this group are favorites
                })
                .ToListAsync();

            return new ToolGroupDto
            {
                Id = "favorites",
                Name = "Favorite Tools",
                Tools = favoriteTools,
                IsExpanded = true // Default to expanded
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve favorite tool group for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateToolPremiumStatusAsync(string toolId, bool isPremium)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools.FindAsync(toolId);
            if (tool == null)
            {
                return false; // Tool not found
            }

            tool.IsPremium = isPremium;
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update premium status for tool {ToolId}", toolId);
            throw;
        }
    }

    public async Task<bool> UpdateToolEnabledStatusAsync(string toolId, bool isEnabled)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools.FindAsync(toolId);
            if (tool == null)
            {
                return false; // Tool not found
            }

            tool.IsEnabled = isEnabled;
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update enabled status for tool {ToolId}", toolId);
            throw;
        }
    }

    public async Task<List<ToolGroupDto>> GetAllToolGroupsForAdminAsync()
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var allToolGroups = await dbContext.ToolGroups
                .Include(tg => tg.Tools)
                .Select(tg => new ToolGroupDto
                {
                    Id = tg.Id,
                    Name = tg.Name,
                    Description = tg.Description,
                    IsExpanded = false,
                    Tools = tg.Tools.Select(t => new ToolDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        IsPremium = t.IsPremium,
                        Slug = t.Slug,
                        Icon = t.Icon,
                        GroupId = t.GroupId,
                        IsEnabled = t.IsEnabled,
                        IsFavorite = false
                    }).ToList()
                })
                .ToListAsync();

            return allToolGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all tool groups for admin");
            throw;
        }
    }

    public async Task<bool> DeleteToolAsync(string toolId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools.FindAsync(toolId);
            if (tool == null)
            {
                return false; // Tool not found
            }

            var toolGroupId = tool.GroupId;

            dbContext.Tools.Remove(tool);
            await dbContext.SaveChangesAsync();

            // Check if the group has no more tools
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
            _logger.LogError(ex, "Failed to delete tool {ToolId}", toolId);
            throw;
        }
    }

    public async Task<ToolDto> AddToolAsync(ToolDto newTool, ToolGroupDto toolGroupDto)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            string toolGroupName = toolGroupDto.Name ?? string.Empty;
            var toolGroup = await dbContext.ToolGroups
                .FirstOrDefaultAsync(tg => tg.Name == toolGroupName);

            if (toolGroup == null)
            {
                toolGroup = new ToolGroup { Name = toolGroupName };
                dbContext.ToolGroups.Add(toolGroup);
                await dbContext.SaveChangesAsync();
            }

            // Check if a tool with the same Slug already exists
            var existingTool = await dbContext.Tools
                .FirstOrDefaultAsync(t => t.Slug == newTool.Slug);

            if (existingTool != null)
            {
                // Return the existing tool as a DTO
                return new ToolDto
                {
                    Id = existingTool.Id,
                    GroupId = existingTool.GroupId,
                    Name = existingTool.Name,
                    Description = existingTool.Description,
                    DllPath = existingTool.DllPath,
                    ConfigPath = existingTool.ConfigPath,
                    Slug = existingTool.Slug,
                    Icon = existingTool.Icon,
                    IsPremium = existingTool.IsPremium,
                    IsEnabled = existingTool.IsEnabled
                };
            }

            var tool = new Tool
            {
                GroupId = toolGroup.Id,
                Name = newTool.Name,
                Description = newTool.Description,
                DllPath = newTool.DllPath,
                ConfigPath = newTool.ConfigPath,
                Slug = newTool.Slug,
                Icon = newTool.Icon,
                IsPremium = false,
                IsEnabled = false,
            };

            dbContext.Tools.Add(tool);
            await dbContext.SaveChangesAsync();

            newTool.Id = tool.Id; // Update the DTO with the new ID
            newTool.IsEnabled = tool.IsEnabled;
            newTool.IsPremium = tool.IsPremium;
            newTool.GroupId = tool.GroupId;
            newTool.Group = new ToolGroupDto
            {
                Id = toolGroup.Id,
                Name = toolGroup.Name,
                Description = toolGroup.Description
            };

            return newTool;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create a new tool");
            throw;
        }
    } 
    

}


