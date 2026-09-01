using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class Top100ListConfiguration : IEntityTypeConfiguration<Top100List>
{
    public void Configure(EntityTypeBuilder<Top100List> builder)
    {
        builder.ToTable("Top100Lists");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Title).IsRequired().HasMaxLength(300);

        builder.HasIndex(l => l.Top100CategoryId);
    }
}
