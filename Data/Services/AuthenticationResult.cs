namespace it_tools.Data.Services;

public class AuthenticationResult
{
    public bool Succeeded { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public bool IsLockedOut { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static AuthenticationResult Success() => new() { Succeeded = true };
    public static AuthenticationResult Failure(string errorMessage) => new() { Succeeded = false, ErrorMessage = errorMessage };
    public static AuthenticationResult TwoFactorRequired() => new() { RequiresTwoFactor = true };
    public static AuthenticationResult LockedOut() => new() { IsLockedOut = true };
}