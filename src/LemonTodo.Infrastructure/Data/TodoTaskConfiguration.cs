using LemonTodo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LemonTodo.Infrastructure.Data;

public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(21);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
    }
}
