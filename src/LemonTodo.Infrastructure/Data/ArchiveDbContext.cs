using LemonTodo.Domain;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Data;

public class ArchiveDbContext : DbContext
{
    public DbSet<TodoTask> Tasks => Set<TodoTask>();

    public ArchiveDbContext(DbContextOptions<ArchiveDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TodoTaskConfiguration());
    }
}
