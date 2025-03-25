using Microsoft.EntityFrameworkCore;

namespace it_tools.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<ToolGroup> ToolGroups { get; set; }
    public DbSet<Tool> Tools { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<FavouriteTool> FavouriteTools { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Tool>()
        .HasOne(t => t.Group)
        .WithMany()
        .HasForeignKey(t => t.GroupId);

        // Quan hệ giữa FavouriteTool và User
        modelBuilder.Entity<FavouriteTool>()
            .HasOne(ft => ft.User)
            .WithMany(u => u.FavouriteTools)
            .HasForeignKey(ft => ft.UserId);

        // Quan hệ giữa FavouriteTool và Tool
        modelBuilder.Entity<FavouriteTool>()
            .HasOne(ft => ft.Tool)
            .WithMany()
            .HasForeignKey(ft => ft.ToolId);
    }
}
