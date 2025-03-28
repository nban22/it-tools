namespace it_tools.Data
{
    public interface ITool
    {
        string Id { get; set; }
        string? Name { get; set; }
        string? Description { get; set; }
        bool IsPremium { get; set; }
        string? Slug { get; set; }
        string? Icon { get; set; }
        bool IsEnabled { get; set; }
        // Không thêm IsFavorite vào đây vì nó chỉ có trong ToolDto
    }
}