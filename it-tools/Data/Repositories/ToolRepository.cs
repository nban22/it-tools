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

    public async Task<ToolDto?> GetToolByIdAsync(string toolId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools
                .Include(t => t.Group)
                .FirstOrDefaultAsync(t => t.Id == toolId);

            if (tool == null)
            {
                return null;
            }

            return new ToolDto
            {
                Id = tool.Id,
                GroupId = tool.GroupId,
                Name = tool.Name,
                Description = tool.Description,
                DllPath = tool.DllPath,
                Slug = tool.Slug,
                Icon = tool.Icon,
                IsPremium = tool.IsPremium,
                IsEnabled = tool.IsEnabled,
                Group = new ToolGroupDto
                {
                    Id = tool.GroupId,
                    Name = tool.Group?.Name ?? string.Empty,
                    Description = tool.Group?.Description ?? string.Empty
                }
            };
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
                .Where(t => t.IsEnabled)
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

    public async Task<List<ToolDto>> GetAllToolsForUserAsync(string userId)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var favoriteToolIds = await dbContext.FavouriteTools
                .Where(ft => ft.UserId == userId)
                .Select(ft => ft.ToolId)
                .ToListAsync();

            var tools = await dbContext.Tools
                .Where(t => t.IsEnabled)
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
                    IsFavorite = favoriteToolIds.Contains(t.Id)
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
                .Where(tg => tg.Tools.Any(t => t.IsEnabled))
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
                            IsFavorite = false
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
                .Where(f => f.UserId == userId && f.Tool != null && f.Tool.IsEnabled == true)
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
                    IsFavorite = true
                })
                .ToListAsync();

            return new ToolGroupDto
            {
                Id = "favorites",
                Name = "Favorite Tools",
                Tools = favoriteTools,
                IsExpanded = true
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
                return false;
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
                return false;
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
                return false;
            }

            var toolGroupId = tool.GroupId;

            var favoriteToolRecords = await dbContext.FavouriteTools
                .Where(ft => ft.ToolId == toolId)
                .ToListAsync();
            if (favoriteToolRecords.Count != 0)
            {
                dbContext.FavouriteTools.RemoveRange(favoriteToolRecords);
                await dbContext.SaveChangesAsync();
            }

            dbContext.Tools.Remove(tool);
            await dbContext.SaveChangesAsync();

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

            var existingTool = await dbContext.Tools
                .FirstOrDefaultAsync(t => t.Slug == newTool.Slug);

            if (existingTool != null)
            {
                string toolPath = Path.GetDirectoryName(newTool.DllPath) ?? string.Empty;
                if (Directory.Exists(toolPath))
                {
                    Directory.Delete(toolPath, true);
                }
                return new ToolDto
                {
                    Id = existingTool.Id,
                    GroupId = existingTool.GroupId,
                    Name = existingTool.Name,
                    Description = existingTool.Description,
                    DllPath = existingTool.DllPath,
                    Slug = existingTool.Slug,
                    Icon = existingTool.Icon,
                    IsPremium = existingTool.IsPremium,
                    IsEnabled = existingTool.IsEnabled,
                    Group = new ToolGroupDto
                    {
                        Id = existingTool.GroupId,
                        Name = toolGroup.Name,
                        Description = toolGroup.Description
                    },
                };
            }

            var tool = new Tool
            {
                GroupId = toolGroup.Id,
                Name = newTool.Name,
                Description = newTool.Description,
                DllPath = newTool.DllPath,
                Slug = newTool.Slug,
                Icon = newTool.Icon,
                IsPremium = false,
                IsEnabled = false,
            };

            dbContext.Tools.Add(tool);
            await dbContext.SaveChangesAsync();

            newTool.Id = tool.Id;
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

    public async Task UpdateToolAsync(ToolDto tool)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var existingTool = await dbContext.Tools
                .Include(t => t.Group)
                .FirstOrDefaultAsync(t => t.Id == tool.Id);

            if (existingTool == null)
            {
                throw new InvalidOperationException($"Tool with ID {tool.Id} not found.");
            }

            existingTool.Name = tool.Name;
            existingTool.Description = tool.Description;
            existingTool.DllPath = tool.DllPath;
            existingTool.Slug = tool.Slug;
            existingTool.Icon = tool.Icon;
            existingTool.IsPremium = tool.IsPremium;
            existingTool.IsEnabled = tool.IsEnabled;

            if (tool.Group != null)
            {
                var toolGroup = await dbContext.ToolGroups
                    .FirstOrDefaultAsync(tg => tg.Name == tool.Group.Name);
                if (toolGroup == null)
                {
                    toolGroup = new ToolGroup
                    {
                        Name = tool.Group?.Name ?? string.Empty,
                        Description = tool.Group?.Description ?? string.Empty
                    };
                    dbContext.ToolGroups.Add(toolGroup);
                    await dbContext.SaveChangesAsync();
                }
                existingTool.GroupId = toolGroup.Id;
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tool with ID {ToolId}", tool.Id);
            throw;
        }
    }

    public async Task<ToolDto?> GetToolBySlugAsync(string slug)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var tool = await dbContext.Tools
                .Include(t => t.Group)
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (tool == null)
            {
                return null;
            }

            return new ToolDto
            {
                Id = tool.Id,
                GroupId = tool.GroupId,
                Name = tool.Name,
                Description = tool.Description,
                DllPath = tool.DllPath,
                Slug = tool.Slug,
                Icon = tool.Icon,
                IsPremium = tool.IsPremium,
                IsEnabled = tool.IsEnabled,
                Group = new ToolGroupDto
                {
                    Id = tool.GroupId,
                    Name = tool.Group?.Name ?? string.Empty,
                    Description = tool.Group?.Description ?? string.Empty
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tool with slug {Slug}", slug);
            throw;
        }
    }
}