using it_tools.Data;

namespace it_tools.Repositories;

public interface IToolRepository
{
    Task<List<Tool>> GetToolsForUnauthorizedAsync();
    Task<List<ToolDto>> GetToolsForUserAsync(string userId);
    // Task<List<ToolGroupDto>> GetToolGroupsForUserAsync(string userId);

    // Task<List<ToolGroupDto>> GetToolGroupsForUnauthorizedAsync();
    
}
