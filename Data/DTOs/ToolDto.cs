namespace it_tools.Data.DTOs;
 
public class ToolDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsPremium { get; set; } = false;
    public string? DllPath { get; set; }
    public string? ConfigPath { get; set; }
    public string? Slug { get; set; } // Slug for the tool (e.g., "tool-name")
    public string? Icon { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool IsFavorite { get; set; } = false;
    public ToolGroupDto? Group { get; set; } // Navigation property
}