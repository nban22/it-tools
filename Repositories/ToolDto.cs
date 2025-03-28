using it_tools.Data;

namespace it_tools.Repositories;

public class ToolDto : ITool
{
    public required string Id { get; set; }
    public string? GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsPremium { get; set; }
    public string? Slug { get; set; }
    public string? Icon { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsFavorite { get; set; } // Thuộc tính bổ sung cho người dùng đã đăng nhập
}