using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<Models.Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Task entity
        modelBuilder.Entity<Models.Task>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.IsCompleted);
            entity.Property(t => t.CreatedAt);
            entity.Property(t => t.UpdatedAt);
        });

        // Seed some initial data
        modelBuilder.Entity<Models.Task>().HasData(
            new Models.Task
            {
                Id = 1,
                Title = "Sample Task 1",
                Description = "This is a sample task for testing",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Models.Task
            {
                Id = 2,
                Title = "Sample Task 2",
                Description = "Another sample task",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );
    }
}
