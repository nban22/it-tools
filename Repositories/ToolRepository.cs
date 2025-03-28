
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
    
    // Lấy danh sách ToolGroups với Tools cho người dùng chưa xác thực
    // public async Task<List<ToolGroupDto>> GetToolGroupsForUnauthorizedAsync()
    // {
    //     try
    //     {
    //         _logger.LogInformation("Fetching tool groups for unauthorized users.");
    //         var toolGroups = await _dbContext.ToolGroups
    //             .Select(g => new ToolGroupDto
    //             {
    //                 Id = g.Id,
    //                 Name = g.Name,
    //                 Description = g.Description,
    //                 Tools = g.Tools
    //                     .Where(t => t.IsEnabled)
    //                     .Select(t => new ToolDto
    //                     {
    //                         Id = t.Id,
    //                         GroupId = t.GroupId,
    //                         Name = t.Name,
    //                         Description = t.Description,
    //                         IsPremium = t.IsPremium,
    //                         Slug = t.Slug,
    //                         Icon = t.Icon,
    //                         IsEnabled = t.IsEnabled,
    //                         IsFavorite = false // Không có favorite cho unauthorized
    //                     }).ToList()
    //             })
    //             .ToListAsync();

    //         _logger.LogInformation("Found {GroupCount} tool groups for unauthorized users.", toolGroups.Count);
    //         return toolGroups;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error fetching tool groups for unauthorized users.");
    //         throw;
    //     }
    // }

    // // Lấy danh sách ToolGroups với Tools cho người dùng đã đăng nhập, bao gồm IsFavorite
    // public async Task<List<ToolGroupDto>> GetToolGroupsForUserAsync(string userId)
    // {
    //     try
    //     {
    //         _logger.LogInformation("Fetching tool groups for user with ID: {UserId}", userId);

    //         // Lấy danh sách các ToolId yêu thích của người dùng
    //         var favoriteToolIds = await _dbContext.FavouriteTools
    //             .Where(ft => ft.UserId == userId)
    //             .Select(ft => ft.ToolId)
    //             .ToListAsync();

    //         // Lấy danh sách ToolGroups và Tools
    //         var toolGroups = await _dbContext.ToolGroups
    //             .Select(g => new ToolGroupDto
    //             {
    //                 Id = g.Id,
    //                 Name = g.Name,
    //                 Description = g.Description,
    //                 Tools = g.Tools
    //                     .Where(t => t.IsEnabled)
    //                     .Select(t => new ToolDto
    //                     {
    //                         Id = t.Id,
    //                         GroupId = t.GroupId,
    //                         Name = t.Name,
    //                         Description = t.Description,
    //                         IsPremium = t.IsPremium,
    //                         Slug = t.Slug,
    //                         Icon = t.Icon,
    //                         IsEnabled = t.IsEnabled,
    //                         IsFavorite = favoriteToolIds.Contains(t.Id)
    //                     }).ToList()
    //             })
    //             .ToListAsync();

    //         _logger.LogInformation("Found {GroupCount} tool groups for user {UserId}.", toolGroups.Count, userId);
    //         return toolGroups;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error fetching tool groups for user {UserId}", userId);
    //         throw;
    //     }
    // }

}

// DTO classes để truyền dữ liệu
public class ToolGroupDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<ToolDto>? Tools { get; set; }
}
