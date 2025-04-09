using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using it_tools.Data.DTOs;
using it_tools.Data.Models;
using it_tools.Data.Repositories;

namespace it_tools.Data.Services;

public interface ISearchService
{
    Task<List<SearchResultDto>> SearchAsync(string query);
}

public class SearchService(ApplicationDbContext context, IToolRepository toolRepository) : ISearchService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IToolRepository _toolRepository = toolRepository;

    public async Task<List<SearchResultDto>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<SearchResultDto>();

        query = query.ToLower().Trim();

        // Search for tools that match the query in name, description, or slug
        var toolResults = await _context.Tools
            .Include(t => t.Group)
            .Where(t => t.IsEnabled &&
                       (t.Name != null && t.Name.ToLower().Contains(query) ||
                        t.Description != null && t.Description.ToLower().Contains(query) ||
                        t.Slug != null && t.Slug.ToLower().Contains(query)))
            .Select(t => new SearchResultDto
            {
                Id = t.Id,
                Name = t.Name ?? "",
                Description = t.Description ?? "",
                Type = "Tool",
                GroupName = t.Group != null ? t.Group.Name : "",
                Icon = t.Icon ?? "",
                Url = $"/tools/{t.Slug}"
            })
            .ToListAsync();

        // Search for tool groups that match the query in name or description
        var groupResults = await _context.ToolGroups
            .Where(g => g.Name.ToLower().Contains(query) ||
                       g.Description.ToLower().Contains(query))
            .Select(g => new SearchResultDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Type = "ToolGroup",
                GroupName = g.Name,
                Icon = "📁",
                Url = "/" // This will navigate to home, where they can expand the group
            })
            .ToListAsync();

        // Combine and order results by relevance (name matches first, then description)
        var combinedResults = toolResults.Concat(groupResults).ToList();
        
        // Rank results by relevance
        return combinedResults
            .OrderByDescending(r => r.Name.ToLower().Contains(query))
            .ThenByDescending(r => r.Name.ToLower().StartsWith(query))
            .ThenBy(r => r.Type == "Tool" ? 0 : 1) // Show tools before groups
            .ThenBy(r => r.Name)
            .ToList();
    }
}

public class SearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Highlight { get; set; } = string.Empty;
}