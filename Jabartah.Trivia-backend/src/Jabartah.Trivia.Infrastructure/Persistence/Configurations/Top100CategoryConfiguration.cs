using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class Top100CategoryConfiguration : IEntityTypeConfiguration<Top100Category>
{
    public void Configure(EntityTypeBuilder<Top100Category> builder)
    {
        builder.ToTable("Top100Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(50);
    }
}
