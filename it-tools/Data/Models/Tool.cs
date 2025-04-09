
namespace it_tools.Data.Models;

public class Tool
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsPremium { get; set; }
    public bool IsEnabled { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    
    // Đường dẫn đến DLL khi lưu trong file system
    public string? DllPath { get; set; }
    
    // Navigation property
    public ToolGroup? Group { get; set; }
}
