using Microsoft.AspNetCore.Components;
using it_tools.ToolDevelopment.Interfaces;

namespace it_tools.ToolDevelopment.Base;
public abstract class ToolComponentBase : ComponentBase, ITool
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Group { get; }
    public abstract string Icon { get; }
    public virtual bool RequiresPremium => false;
    
    // Mặc định lấy Slug từ tên
    public virtual string Slug => Name?.Replace(" ", "-").ToLowerInvariant() ?? "";
    
    public virtual Task Initialize() => Task.CompletedTask;
}