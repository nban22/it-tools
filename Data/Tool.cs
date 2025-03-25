public class Tool
{
    public required string Id { get; set; }
    public string? GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsPremium { get; set; }
    public bool IsEnabled { get; set; }
    public string? ToolKey { get; set; }

    // Navigation property
    public ToolGroup? Group { get; set; }
}