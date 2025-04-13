// Data/DbInitializer.cs
using Microsoft.EntityFrameworkCore;
using it_tools.Data.Models;

namespace it_tools.Data;

public static class DbInitializer
{
    public static async Task SeedAdminUser(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        
        // Check if admin user already exists
        if (await context.Admins.AnyAsync())
        {
            return; // Admin already seeded
        }

        // Create admin user
        var adminUser = new Admin
        {
            Id = Guid.NewGuid().ToString(),
            Username = "admin",
            Email = "admin@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") // You should change this in production
        };

        var adminUser1 = new Admin
        {
            Id = Guid.NewGuid().ToString(),
            Username = "admin1",
            Email = "admin1@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") // You should change this in production
        };
        var adminUser2 = new Admin
        {
            Id = Guid.NewGuid().ToString(),
            Username = "admin2",
            Email = "admin2@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") // You should change this in production
        };

        context.Admins.Add(adminUser);
        context.Admins.Add(adminUser1);
        context.Admins.Add(adminUser2);
        await context.SaveChangesAsync();
    }
}