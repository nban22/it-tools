using Microsoft.EntityFrameworkCore;
using it_tools.Data.Models;

namespace it_tools.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ToolGroup> ToolGroups { get; set; }
    public DbSet<Tool> Tools { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<FavouriteTool> FavouriteTools { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Định nghĩa khóa chính cho các bảng
        modelBuilder.Entity<ToolGroup>().HasKey(tg => tg.Id);
        modelBuilder.Entity<Tool>().HasKey(t => t.Id);
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<FavouriteTool>().HasKey(ft => ft.Id);
        modelBuilder.Entity<Admin>().HasKey(a => a.Id);

        // Email configuration - nvarchar(255) and not null
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasColumnType("varchar(255)");

        modelBuilder.Entity<Admin>()
            .Property(a => a.Email)
            .IsRequired()
            .HasColumnType("varchar(255)");

        // Tool relationships
        modelBuilder.Entity<Tool>()
            .HasOne(t => t.Group)
            .WithMany(g => g.Tools)
            .HasForeignKey(t => t.GroupId)
            .IsRequired(); // GroupId not null

        modelBuilder.Entity<Tool>()
            .Property(t => t.DllPath)
            .IsRequired(); // DllPath not null

        // Quan hệ giữa FavouriteTool và User
        modelBuilder.Entity<FavouriteTool>()
            .HasOne(ft => ft.User)
            .WithMany(u => u.FavouriteTools)
            .HasForeignKey(ft => ft.UserId)
            .IsRequired(); // UserId not null

        // Quan hệ giữa FavouriteTool và Tool
        modelBuilder.Entity<FavouriteTool>()
            .HasOne(ft => ft.Tool)
            .WithMany()
            .HasForeignKey(ft => ft.ToolId)
            .IsRequired(); // ToolId not null

        // Add unique constraint for UserId and ToolId in FavouriteTool
        modelBuilder.Entity<FavouriteTool>()
            .HasIndex(ft => new { ft.UserId, ft.ToolId })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}