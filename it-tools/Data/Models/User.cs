namespace it_tools.Data.Models;
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed to Guid for UUID
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public required string Email { get; set; } // Made required (not null)
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool PremiumRequest { get; set; }

    // Navigation property
    public ICollection<FavouriteTool>? FavouriteTools { get; set; }
}