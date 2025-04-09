using it_tools.Data.DTOs;

namespace it_tools.Data.Repositories;

public interface IToolGroupRepository
{
    Task<List<ToolGroupDto>> GetAllToolGroups();
    Task<ToolGroupDto?> GetFavoriteToolGroup(string userId);
    Task<ToolGroupDto?> GetToolGroupById(string groupId);
}