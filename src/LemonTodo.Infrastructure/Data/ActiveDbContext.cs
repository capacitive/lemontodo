using LemonTodo.Domain;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Infrastructure.Data;

public class ActiveDbContext : DbContext
{
    public DbSet<TodoTask> Tasks => Set<TodoTask>();

    public ActiveDbContext(DbContextOptions<ActiveDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TodoTaskConfiguration());
    }
}
