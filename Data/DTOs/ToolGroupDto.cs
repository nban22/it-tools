namespace it_tools.Data.DTOs;
public class ToolGroupDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<ToolDto>? Tools { get; set; }
    
    public bool IsExpanded { get; set; } = false;
}