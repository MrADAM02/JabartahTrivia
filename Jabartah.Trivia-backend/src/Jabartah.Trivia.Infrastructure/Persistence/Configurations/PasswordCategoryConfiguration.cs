using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class PasswordCategoryConfiguration : IEntityTypeConfiguration<PasswordCategory>
{
    public void Configure(EntityTypeBuilder<PasswordCategory> builder)
    {
        builder.ToTable("PasswordCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(50);
    }
}
