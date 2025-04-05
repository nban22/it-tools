
namespace it_tools.Data.Models;

public class Tool
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsPremium { get; set; }
    public string? DllPath { get; set; } // Path to the DLL file
    public string? ConfigPath { get; set; } // Path to the optional config file
    public string? Slug { get; set; } // Slug for the tool (e.g., "tool-name")
    public string? Icon { get; set; } // Category of the tool (e.g., "AI", "Data", etc.)
    public bool IsEnabled { get; set; }
    // Navigation property
    public ToolGroup? Group { get; set; }
}