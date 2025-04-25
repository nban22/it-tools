namespace it_tools.Data.Models;
public class FavouriteTool
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed to Guid for UUID
    public Guid UserId { get; set; } // Changed to Guid and is required by default
    public Guid ToolId { get; set; } // Changed to Guid and is required by default

    // Navigation properties
    public User? User { get; set; }
    public Tool? Tool { get; set; }
}