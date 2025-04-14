using it_tools.Data.DTOs;
using it_tools.Data.Models;

namespace it_tools.Data.Repositories;

public interface IToolRepository
{
    Task<ToolDto?> GetToolByIdAsync(string toolId); // Thay đổi từ Tool? sang ToolDto?
    Task<List<ToolDto>> GetAllToolsForUnauthorizedAsync();
    Task<List<ToolDto>> GetAllToolsForUserAsync(string userId);
    Task<List<ToolGroupDto>> GetAllToolGroups();
    Task<ToolGroupDto> GetFavoriteToolGroup(string userId);
    Task<bool> UpdateToolPremiumStatusAsync(string toolId, bool isPremium);
    Task<bool> UpdateToolEnabledStatusAsync(string toolId, bool isEnabled);
    Task<List<ToolGroupDto>> GetAllToolGroupsForAdminAsync();
    Task<bool> DeleteToolAsync(string toolId);
    Task<ToolDto> AddToolAsync(ToolDto newTool, ToolGroupDto toolGroupDto);
    Task UpdateToolAsync(ToolDto tool);
    Task<ToolDto?> GetToolBySlugAsync(string slug);
}