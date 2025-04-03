namespace it_tools.Data.Models;
public class FavouriteTool
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string ToolId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Tool? Tool { get; set; }
}