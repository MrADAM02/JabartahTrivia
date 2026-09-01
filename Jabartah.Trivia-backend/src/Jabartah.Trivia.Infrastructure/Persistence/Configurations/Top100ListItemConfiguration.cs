using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class Top100ListItemConfiguration : IEntityTypeConfiguration<Top100ListItem>
{
    public void Configure(EntityTypeBuilder<Top100ListItem> builder)
    {
        builder.ToTable("Top100ListItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Label).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Position).IsRequired();

        builder.PrimitiveCollection(i => i.AlternateSpellings).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => i.Top100ListId);
        builder.HasIndex(i => new { i.Top100ListId, i.Position }).IsUnique();
    }
}
