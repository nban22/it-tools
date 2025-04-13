// Pages/Auth/AdminLoginHandler.cshtml.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using it_tools.Data.Services;

namespace it_tools.Pages.Auth;

public class AdminLoginHandlerModel(IAdminService adminService) : PageModel
{
    private readonly IAdminService _adminService = adminService;

    public async Task<IActionResult> OnGetAsync(string email, bool rememberMe, string returnUrl)
    {
        // Get admin by email
        var admin = await _adminService.GetAdminByEmailAsync(email);
        if (admin == null)
        {
            return BadRequest("Admin not found.");
        }

        // Create claims for admin
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin") // Set role as Admin
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Set cookie with SignInAsync
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = rememberMe });

        // Redirect to requested page
        return LocalRedirect(returnUrl ?? "/admin/dashboard");
    }
}