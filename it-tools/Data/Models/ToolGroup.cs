namespace it_tools.Data.Models;
public class ToolGroup
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed to Guid for UUID
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Tool> Tools { get; set; } = [];
}