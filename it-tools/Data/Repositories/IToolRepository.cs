using it_tools.Data.Models;

namespace it_tools.Data.Repositories;

public interface IToolRepository
{
    Task<Tool?> GetToolEntityByIdAsync(Guid toolId);
    Task<Tool?> GetToolEntityBySlugAsync(string slug);
    Task<List<Tool>> GetAllEnabledToolEntitiesAsync();
    Task<List<Tool>> GetAllToolEntitiesAsync();
    Task<List<ToolGroup>> GetAllToolGroupEntitiesAsync();
    Task<List<ToolGroup>> GetAllEnabledToolGroupEntitiesAsync();
    Task<List<Guid>> GetFavoriteToolIdsForUserAsync(Guid userId);
    Task<List<Tool>> GetFavoriteToolEntitiesForUserAsync(Guid userId);
    Task<ToolGroup?> GetToolGroupByNameAsync(string name);
    Task<ToolGroup> CreateToolGroupAsync(ToolGroup toolGroup);
    Task<Tool> CreateToolAsync(Tool tool);
    Task UpdateToolAsync(Tool tool);
    Task<bool> DeleteToolAsync(Guid toolId);
    Task<bool> UpdateToolPropertyAsync(Guid toolId, string propertyName, object value);
}