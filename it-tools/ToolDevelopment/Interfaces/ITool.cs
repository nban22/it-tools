namespace it_tools.ToolDevelopment.Interfaces;
public interface ITool
{
    string Name { get; }
    string Description { get; }
    string Group { get; }
    string Icon { get; } 
    bool RequiresPremium { get; }
    string Slug { get; } // Cho phép tool tự xác định slug
    
    // Các method tùy chọn
    Task Initialize();
}