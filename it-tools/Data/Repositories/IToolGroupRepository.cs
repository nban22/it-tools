using it_tools.Data.DTOs;

namespace it_tools.Data.Repositories;

public interface IToolGroupRepository
{
    Task<List<ToolGroupDto>> GetAllToolGroups();
    Task<ToolGroupDto?> GetFavoriteToolGroup(Guid userId);
    Task<ToolGroupDto?> GetToolGroupById(Guid groupId);
}