namespace it_tools.Data.DTOs;
public class ToolGroupDto
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<ToolDto>? Tools { get; set; }
    
    public bool IsExpanded { get; set; } = false;
}