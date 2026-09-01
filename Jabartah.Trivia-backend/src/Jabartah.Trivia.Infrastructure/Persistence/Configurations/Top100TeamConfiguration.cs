using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class Top100TeamConfiguration : IEntityTypeConfiguration<Top100Team>
{
    public void Configure(EntityTypeBuilder<Top100Team> builder)
    {
        builder.ToTable("Top100Teams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Score).IsRequired();
        builder.Property(t => t.TurnOrder).IsRequired();
        builder.HasIndex(t => t.Top100GameSessionId);
    }
}
