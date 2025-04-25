namespace it_tools.Data.Models;
public class Admin
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed to Guid for UUID
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public required string Email { get; set; } // Made required (not null)
}