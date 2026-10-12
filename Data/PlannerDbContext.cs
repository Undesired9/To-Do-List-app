using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Models;

namespace To_Do_list_app.Data;

public class PlannerDbContext : DbContext
{
    public PlannerDbContext(DbContextOptions<PlannerDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskItemCategory> TaskItemCategories => Set<TaskItemCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var task = modelBuilder.Entity<TaskItem>();
        task.HasKey(t => t.Id);
        task.Property(t => t.Title).IsRequired().HasMaxLength(200);
        task.Property(t => t.Description).HasMaxLength(1000);
        task.Property(t => t.DueDate).IsRequired();
        task.HasMany(t => t.Categories)
            .WithOne(c => c.TaskItem)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        var category = modelBuilder.Entity<TaskItemCategory>();
        category.HasKey(c => new { c.TaskItemId, c.Category });
        category.Property(c => c.Category).HasConversion<string>().HasMaxLength(30);

        base.OnModelCreating(modelBuilder);
    }
}
