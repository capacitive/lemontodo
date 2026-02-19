using LemonTodo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LemonTodo.Infrastructure.Data;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasMaxLength(21);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).HasMaxLength(200);
        builder.Property(u => u.TotpSecret).HasMaxLength(500);
        builder.Property(u => u.ApiKeyHash).HasMaxLength(128);
        builder.Property(u => u.BoardPreferencesJson).HasMaxLength(4000);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.OwnsMany(u => u.ExternalLogins, el =>
        {
            el.WithOwner().HasForeignKey(e => e.UserId);
            el.HasKey(e => new { e.Provider, e.UserId });
            el.Property(e => e.Provider).HasMaxLength(50);
            el.Property(e => e.ProviderUserId).HasMaxLength(200);
            el.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
        });
    }
}
