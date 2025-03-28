// /Pages/Auth/LoginHandler.cshtml.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using it_tools.Services;

namespace it_tools.Pages.Auth;

public class LoginHandlerModel : PageModel
{
    private readonly UserService _userService;

    public LoginHandlerModel(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync(string email, bool rememberMe, string returnUrl)
    {
        // Lấy thông tin người dùng từ email
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        // Tạo claims cho người dùng
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Set cookie bằng SignInAsync
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = rememberMe });

        // Redirect đến trang mong muốn
        return LocalRedirect(returnUrl ?? "/");
    }
}