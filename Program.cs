using Microsoft.EntityFrameworkCore;
using it_tools.Components;
using it_tools.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using it_tools.Components.Auth;
using it_tools.Services;
using it_tools.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();

builder.Services.AddCascadingAuthenticationState();


builder.Services.AddScoped<RedirectManager>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<UserRepository>();



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied"; // Trang khi bị từ chối truy cập
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Thời gian hết hạn cookie
    });

// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
// });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Thêm middleware xác thực
app.UseAuthorization();  // Thêm middleware phân quyền

app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();
