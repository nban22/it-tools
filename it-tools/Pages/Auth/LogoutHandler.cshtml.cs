using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace it_tools.Pages.Auth;

public class LogoutHandlerModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        // Sign out the user
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Determine if the user was an admin
        bool isAdmin = User.IsInRole("Admin");
        
        // Redirect based on user type
        if (isAdmin)
        {
            return LocalRedirect("/admin/login");
        }
        
        return LocalRedirect("/");
    }
}