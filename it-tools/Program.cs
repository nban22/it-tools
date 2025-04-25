using Microsoft.EntityFrameworkCore;
using it_tools.Components;
using it_tools.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using it_tools.Components.Auth;
using it_tools.Data.Services;
using it_tools.Data.Repositories;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection; // Thêm namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<RedirectManager>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IToolService, ToolService>();
builder.Services.AddScoped<ToolStateService>();
builder.Services.AddScoped<IToolGroupRepository, ToolGroupRepository>();
builder.Services.AddScoped<ToolAssemblyService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IFavouriteToolRepository, FavouriteToolRepository>();
builder.Services.AddScoped<IFavouriteToolService, FavouriteToolService>();

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddSingleton<CleanupService>();
builder.Services.AddHostedService<CleanupService>();

// Đăng ký IFileSystem
builder.Services.AddSingleton<IFileSystem, FileSystem>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var contextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await DbInitializer.SeedAdminUser(contextFactory);
}

if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();