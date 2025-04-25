using it_tools.Data.DTOs;

namespace it_tools.Data.Services
{
    public interface IFavouriteToolService
    {
        Task AddFavouriteToolAsync(string userId, string toolId);
        Task RemoveFavouriteToolAsync(string userId, string toolId);
        Task<List<ToolDto>> GetFavouriteToolsForUserAsync(string userId);
        Task<ToolGroupDto> GetFavouriteToolsAsGroupAsync(string userId);
    }
}