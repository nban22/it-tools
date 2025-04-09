using Microsoft.EntityFrameworkCore;
using it_tools.Data.DTOs;
using it_tools.Data.Models;

namespace it_tools.Data.Repositories;
interface IUserRepository
{
    Task AddFavoriteTool(string userId, string toolId);

    Task RemoveFavoriteTool(string userId, string toolId);
    Task<List<ToolDto>> GetFavoriteTools(string userId);

    Task<UserDto?> GetUserById(string userId);
}