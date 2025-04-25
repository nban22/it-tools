using it_tools.Data.Models;

namespace it_tools.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserEntityByIdAsync(Guid userId);
    Task<User?> GetUserEntityByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> UpdateUserPropertyAsync(Guid userId, string propertyName, object value);
    Task<bool> DeleteUserAsync(Guid userId);
}