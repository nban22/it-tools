using it_tools.Data.DTOs;

namespace it_tools.Data.Services;

public class ToolStateService
{
    // Event that components can subscribe to
    public event Action<ToolDto>? OnToolFavoriteChanged;

    // Method to notify subscribers when a tool's favorite status changes
    public void NotifyToolFavoriteChanged(ToolDto tool)
    {
        OnToolFavoriteChanged?.Invoke(tool);
    }
}