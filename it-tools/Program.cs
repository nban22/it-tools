using Microsoft.EntityFrameworkCore;
using it_tools.Components;
using it_tools.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using it_tools.Components.Auth;
using it_tools.Data.Services;
using it_tools.Data.Repositories;

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
        options.AccessDeniedPath = "/access-denied"; // Trang khi bị từ chối truy cập
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Thời gian hết hạn cookie
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

builder.Services.AddScoped<IAdminService, AdminService>();

// Đăng ký CleanupService
builder.Services.AddSingleton<CleanupService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var contextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await DbInitializer.SeedAdminUser(contextFactory);
}

// Gọi CleanupService để xóa các thư mục "pending deletions" khi ứng dụng khởi động
app.Services.GetRequiredService<CleanupService>().CleanupPendingDeletions();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Có thể thêm middleware hoặc logic cho môi trường Development
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Thêm middleware xác thực
app.UseAuthorization(); // Thêm middleware phân quyền

app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();