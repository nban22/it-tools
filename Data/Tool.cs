using it_tools.Data;

public class Tool : ITool
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


    public string? ToolKey { get; set; }
    public string? ToolUrl { get; set; } // Unique URL for the tool (e.g., "/tools/{ToolKey}")

    // Navigation property
    public ToolGroup? Group { get; set; }

    public string? Title { get; set; }
    public string? IconSvg { get; set; } // SVG path for the main icon
    public bool IsFavorite { get; set; } // Favorite status
    public string? SpecialIconSvg { get; set; } // SVG path for special icon (if any)
    public string? SpecialIconColor { get; set; } // TailwindCSS color for special icon
}