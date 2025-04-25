using it_tools.Data.DTOs;
using it_tools.Data.Models;
using it_tools.Data.Repositories;

namespace it_tools.Data.Services;

public class FavouriteToolService(
    IFavouriteToolRepository favouriteToolRepository,
    ILogger<FavouriteToolService> logger) : IFavouriteToolService
{
    private readonly IFavouriteToolRepository _favouriteToolRepository = favouriteToolRepository;
    private readonly ILogger<FavouriteToolService> _logger = logger;

    public async Task AddFavouriteToolAsync(string userId, string toolId)
    {
        ValidateIds(userId, toolId, out var userGuid, out var toolGuid);

        // Check if the favorite already exists
        if (await _favouriteToolRepository.ExistsAsync(userGuid, toolGuid))
        {
            return; // Already exists, nothing to do
        }

        await _favouriteToolRepository.AddAsync(userGuid, toolGuid);
    }

    public async Task<ToolGroupDto> GetFavouriteToolsAsGroupAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid) || userGuid == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID");
        }

        var tools = await _favouriteToolRepository.GetAllByUserIdAsync(userGuid);
        var toolDtos = tools.Select(MapToToolDto).ToList();

        return new ToolGroupDto
        {
            Tools = toolDtos,
            Name = "Your Favourite Tools",
            Description = "A collection of your favourite tools."
        };
    }

    public async Task<List<ToolDto>> GetFavouriteToolsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid) || userGuid == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID");
        }

        var tools = await _favouriteToolRepository.GetAllByUserIdAsync(userGuid);
        return tools.Select(MapToToolDto).ToList();
    }

    public async Task RemoveFavouriteToolAsync(string userId, string toolId)
    {
        ValidateIds(userId, toolId, out var userGuid, out var toolGuid);
        await _favouriteToolRepository.RemoveAsync(userGuid, toolGuid);
    }

    private static void ValidateIds(string userId, string toolId, out Guid userGuid, out Guid toolGuid)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty");
        }

        if (string.IsNullOrEmpty(toolId))
        {
            throw new ArgumentException("Tool ID cannot be null or empty");
        }

        if (!Guid.TryParse(userId, out userGuid) || userGuid == Guid.Empty)
        {
            throw new ArgumentException("User ID is not a valid GUID");
        }

        if (!Guid.TryParse(toolId, out toolGuid) || toolGuid == Guid.Empty)
        {
            throw new ArgumentException("Tool ID is not a valid GUID");
        }
    }

    private static ToolDto MapToToolDto(Tool tool)
    {
        if (tool == null)
        {
            return new ToolDto();
        }

        return new ToolDto
        {
            Id = tool.Id.ToString(),
            Name = tool.Name,
            Description = tool.Description,
            Slug = tool.Slug,
            Icon = tool.Icon,
            IsEnabled = tool.IsEnabled,
            IsPremium = tool.IsPremium,
        };
    }
}