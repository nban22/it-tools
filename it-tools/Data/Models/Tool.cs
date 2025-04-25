namespace it_tools.Data.Models;
public class Tool
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed to Guid for UUID
    public Guid GroupId { get; set; } // Changed to Guid and is required by default
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsPremium { get; set; }
    public bool IsEnabled { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }

    // Made required (not null)
    public required string DllPath { get; set; }

    // Navigation property
    public ToolGroup? Group { get; set; }
}