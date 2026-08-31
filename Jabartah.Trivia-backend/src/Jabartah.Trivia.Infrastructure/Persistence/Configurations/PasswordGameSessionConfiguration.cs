using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class PasswordGameSessionConfiguration : IEntityTypeConfiguration<PasswordGameSession>
{
    public void Configure(EntityTypeBuilder<PasswordGameSession> builder)
    {
        builder.ToTable("PasswordGameSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CompletedAt);

        builder.HasMany(s => s.Teams)
            .WithOne()
            .HasForeignKey(t => t.PasswordGameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Teams).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Rounds)
            .WithOne()
            .HasForeignKey(r => r.PasswordGameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Rounds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.PrimitiveCollection(s => s.CategoryIds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PasswordRoundConfiguration : IEntityTypeConfiguration<PasswordRound>
{
    public void Configure(EntityTypeBuilder<PasswordRound> builder)
    {
        builder.ToTable("PasswordRounds");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Outcome)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ResolvedAt);

        builder.HasIndex(r => new { r.PasswordGameSessionId, r.RoundNumber }).IsUnique();
    }
}
