using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using System.Security.Claims;

namespace it_tools.Components.Admin.Auth;

public class AdminAuthorizationFilter : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity!.IsAuthenticated || !user.IsInRole("Admin"))
        {
            var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            if (string.IsNullOrEmpty(returnUrl))
            {
                NavigationManager.NavigateTo("/admin/login", forceLoad: true);
            }
            else
            {
                NavigationManager.NavigateTo($"/admin/login?returnUrl=/{returnUrl}", forceLoad: true);
            }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }
}