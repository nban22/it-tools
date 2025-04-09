using it_tools.Data.Models;

namespace it_tools.Data.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool PremiumRequest { get; set; }
    
    public List<ToolDto> FavoriteTools { get; set; } = new List<ToolDto>();
}


// public class User
// {
//     public required string Id { get; set; } 
//     public string? Username { get; set; }
//     public string? PasswordHash { get; set; }
//     public string? Email { get; set; }
//     public bool IsPremium { get; set; }
//     public DateTime CreatedAt { get; set; }
//     public bool PremiumRequest { get; set; }

//     // Navigation property
//     public ICollection<FavouriteTool>? FavouriteTools { get; set; }
// }