
using it_tools.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace it_tools.Repositories;

public class ToolRepository : IToolRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ToolRepository> _logger;

    public ToolRepository(ApplicationDbContext dbContext, ILogger<ToolRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Lấy danh sách Tools cho người dùng chưa xác thực (unauthorized)
    public async Task<List<Tool>> GetToolsForUnauthorizedAsync()
    {
        try
        {
            _logger.LogInformation("Fetching tools for unauthorized users.");
            var tools = await _dbContext.Tools
                .Where(t => t.IsEnabled) // Chỉ lấy các tool đang bật
                .ToListAsync();

            _logger.LogInformation("Found {ToolCount} enabled tools for unauthorized users.", tools.Count);
            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tools for unauthorized users.");
            throw;
        }
    }

    // Lấy danh sách Tools cho người dùng đã đăng nhập, bao gồm IsFavorite
    public async Task<List<ToolDto>> GetToolsForUserAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching tools for user with ID: {UserId}", userId);

            // Lấy danh sách các ToolId yêu thích của người dùng
            var favoriteToolIds = await _dbContext.FavouriteTools
                .Where(ft => ft.UserId == userId)
                .Select(ft => ft.ToolId)
                .ToListAsync();

            // Lấy danh sách Tools và kiểm tra IsFavorite
            var tools = await _dbContext.Tools
                .Where(t => t.IsEnabled) // Chỉ lấy các tool đang bật
                .Select(t => new ToolDto
                {
                    Id = t.Id,
                    GroupId = t.GroupId,
                    Name = t.Name,
                    Description = t.Description,
                    IsPremium = t.IsPremium,
                    Slug = t.Slug,
                    Icon = t.Icon,
                    IsEnabled = t.IsEnabled,
                    IsFavorite = favoriteToolIds.Contains(t.Id) // Kiểm tra xem Tool có trong danh sách yêu thích không
                })
                .ToListAsync();

            _logger.LogInformation("Found {ToolCount} enabled tools for user {UserId}, with favorite status.", tools.Count, userId);
            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tools for user {UserId}", userId);
            throw;
        }
    }
}