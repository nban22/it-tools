using it_tools.Data.DTOs;
using Microsoft.EntityFrameworkCore;


namespace it_tools.Data.Repositories
{
    public class ToolGroupRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IToolGroupRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task<List<ToolGroupDto>> GetAllToolGroups()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var groups = await context.ToolGroups
                    .Include(g => g.Tools)
                    .ToListAsync();

                return groups.Select(g => new ToolGroupDto
                {
                    Id = g.Id.ToString(),
                    Name = g.Name,
                    Tools = g.Tools?.Select(t => new ToolDto
                    {
                        Id = t.Id.ToString(),
                        Name = t.Name,
                        Description = t.Description,
                        Slug = t.Slug,
                        Icon = t.Icon
                    }).ToList() ?? new List<ToolDto>()
                }).ToList();
            }
        }

        public async Task<ToolGroupDto?> GetFavoriteToolGroup(Guid userId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var favoriteTools = await context.FavouriteTools
                    .Where(uf => uf.UserId == userId)
                    .Join(context.Tools,
                        uf => uf.ToolId,
                        t => t.Id,
                        (uf, t) => new ToolDto
                        {
                            Id = t.Id.ToString(),
                            Name = t.Name,
                            Description = t.Description,
                            Slug = t.Slug,
                            Icon = t.Icon,
                            IsFavorite = true
                        })
                    .ToListAsync();

                if (!favoriteTools.Any())
                    return null;

                return new ToolGroupDto
                {
                    Id = "favorites",
                    Name = "Favorites",
                    IsExpanded = true,
                    Tools = favoriteTools
                };
            }
        }

        public async Task<ToolGroupDto?> GetToolGroupById(Guid groupId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var group = await context.ToolGroups
                    .Include(g => g.Tools)
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                if (group == null)
                    return null;

                return new ToolGroupDto
                {
                    Id = group.Id.ToString(),
                    Name = group.Name,
                    Tools = group.Tools?.Select(t => new ToolDto
                    {
                        Id = t.Id.ToString(),
                        Name = t.Name,
                        Description = t.Description,
                        Slug = t.Slug,
                        Icon = t.Icon
                    }).ToList() ?? new List<ToolDto>()
                };
            }
        }
    }
}