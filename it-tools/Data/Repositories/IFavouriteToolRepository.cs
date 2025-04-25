using it_tools.Data.Models;

namespace it_tools.Data.Repositories
{
    public interface IFavouriteToolRepository
    {
        Task AddAsync(Guid userId, Guid toolId);
        Task RemoveAsync(Guid userId, Guid toolId);
        Task<List<Tool?>> GetAllByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, Guid toolId);
    }
}