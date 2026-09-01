using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class Top100GameSessionConfiguration : IEntityTypeConfiguration<Top100GameSession>
{
    public void Configure(EntityTypeBuilder<Top100GameSession> builder)
    {
        builder.ToTable("Top100GameSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.RoundsPerTeam).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CompletedAt);

        builder.HasMany(s => s.Teams)
            .WithOne()
            .HasForeignKey(t => t.Top100GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Teams).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Rounds)
            .WithOne()
            .HasForeignKey(r => r.Top100GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Rounds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.PrimitiveCollection(s => s.CategoryIds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class Top100RoundConfiguration : IEntityTypeConfiguration<Top100Round>
{
    public void Configure(EntityTypeBuilder<Top100Round> builder)
    {
        builder.ToTable("Top100Rounds");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.CurrentTurnTeamId).IsRequired();
        builder.Property(r => r.GuessesMade).IsRequired();
        builder.Property(r => r.MaxGuesses).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ResolvedAt);

        builder.PrimitiveCollection(r => r.GuessedItemIds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => new { r.Top100GameSessionId, r.RoundNumber }).IsUnique();
    }
}
