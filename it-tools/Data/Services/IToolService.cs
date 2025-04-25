using it_tools.Data.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace it_tools.Data.Services;

public interface IToolService
{
    Task<ToolDto?> GetToolByIdAsync(string toolId);
    Task<ToolDto?> GetToolBySlugAsync(string slug);
    Task<List<ToolDto>> GetAllToolsForUnauthorizedAsync();
    Task<List<ToolDto>> GetAllToolsForUserAsync(string userId);
    Task<List<ToolGroupDto>> GetAllToolGroups();
    Task<ToolGroupDto> GetFavoriteToolGroup(string userId);
    Task<List<ToolGroupDto>> GetAllToolGroupsForAdminAsync();
    Task<bool> UpdateToolPremiumStatusAsync(string toolId, bool isPremium);
    Task<bool> UpdateToolEnabledStatusAsync(string toolId, bool isEnabled);
    Task<bool> DeleteToolAsync(string toolId);
    Task<ToolDto?> UploadToolAsync(IBrowserFile file);
    Task UpdateToolAsync(ToolDto toolDto);
}