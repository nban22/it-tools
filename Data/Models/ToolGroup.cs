namespace it_tools.Data.Models;
public class ToolGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
     public List<Tool> Tools { get; set; } = [];
}