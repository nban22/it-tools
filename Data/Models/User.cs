namespace it_tools.Data.Models;

public class User
{
    public required string Id { get; set; } 
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool PremiumRequest { get; set; }

    // Navigation property
    public ICollection<FavouriteTool>? FavouriteTools { get; set; }
}