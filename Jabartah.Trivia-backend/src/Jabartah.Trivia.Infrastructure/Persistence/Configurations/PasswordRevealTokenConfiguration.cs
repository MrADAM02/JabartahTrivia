using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class PasswordRevealTokenConfiguration : IEntityTypeConfiguration<PasswordRevealToken>
{
    public void Configure(EntityTypeBuilder<PasswordRevealToken> builder)
    {
        builder.ToTable("PasswordRevealTokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token).IsRequired().HasMaxLength(64);
        builder.HasIndex(t => t.Token).IsUnique();

        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.ConsumedAt);

        builder.HasIndex(t => t.PasswordRoundId);
    }
}
