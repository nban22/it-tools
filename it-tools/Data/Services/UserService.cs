using Microsoft.EntityFrameworkCore;
using it_tools.Data.Models;
using it_tools.Data.Repositories;
using it_tools.Data.DTOs;

namespace it_tools.Data.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFavouriteToolRepository _favouriteToolRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IFavouriteToolRepository favouriteToolRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _favouriteToolRepository = favouriteToolRepository;
        _logger = logger;
    }

    public async Task<bool> RegisterUserAsync(string email, string password, string? userName = null)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetUserEntityByEmailAsync(email);
            if (existingUser != null)
            {
                return false;
            }

            // Hash password and create new user
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var extractedUsername = email.Split('@')[0];
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = userName ?? extractedUsername,
                Email = email,
                PasswordHash = hashedPassword,
                IsPremium = false,
                CreatedAt = DateTime.UtcNow,
                PremiumRequest = false,
            };

            await _userRepository.CreateUserAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user with email {Email}", email);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserEntityByEmailAsync(email);
        return user != null ? MapUserEntityToDto(user) : null;
    }

    public async Task<AuthenticationResult> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            var user = await _userRepository.GetUserEntityByEmailAsync(email);
            if (user == null)
            {
                return AuthenticationResult.Failure("Email or password is incorrect.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return AuthenticationResult.Failure("Email or password is incorrect.");
            }

            return AuthenticationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for email {Email}", email);
            return AuthenticationResult.Failure("An error occurred during authentication.");
        }
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format.", nameof(userId));
            }

            var user = await _userRepository.GetUserEntityByIdAsync(userGuid);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            return MapUserEntityToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user with ID {UserId}", userId);
            throw new ApplicationException("An error occurred while retrieving the user.", ex);
        }
    }

    public async Task<bool> SetUserPremiumStatusAsync(string userId, bool isPremium)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format.", nameof(userId));
            }

            var result = await _userRepository.UpdateUserPropertyAsync(userGuid, nameof(User.IsPremium), isPremium);
            if (result)
            {
                // Clear any pending premium requests
                await _userRepository.UpdateUserPropertyAsync(userGuid, nameof(User.PremiumRequest), false);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set premium status for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RequestPremiumStatusAsync(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid user ID format.", nameof(userId));
            }

            return await _userRepository.UpdateUserPropertyAsync(userGuid, nameof(User.PremiumRequest), true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request premium status for user {UserId}", userId);
            throw;
        }
    }

    // Helper methods
    private UserDto MapUserEntityToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            PremiumRequest = user.PremiumRequest,
            FavoriteTools = user.FavouriteTools?.Select(ft => new ToolDto
            {
                Id = ft.Tool != null ? ft.Tool.Id.ToString() : string.Empty,
                Name = ft.Tool != null ? ft.Tool.Name : string.Empty,
                Description = ft.Tool != null ? ft.Tool.Description : string.Empty,
                Slug = ft.Tool?.Slug,
                Icon = ft.Tool?.Icon,
                IsEnabled = ft.Tool?.IsEnabled ?? false,
                IsPremium = ft.Tool?.IsPremium ?? false,
                IsFavorite = true
            }).ToList() ?? new List<ToolDto>()
        };
    }
}