using Jabartah.Trivia.Domain.Top100Game;
using Jabartah.Trivia.Domain.Users;
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

        builder.Property(s => s.GuessesPerTeam).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CompletedAt);

        builder.HasOne<User>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(s => s.UserId);

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

        builder.HasMany(r => r.Guesses)
            .WithOne()
            .HasForeignKey(g => g.Top100RoundId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Guesses).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Exactly one round is ever created per session now, so a unique index on the
        // session FK alone (no RoundNumber anymore) enforces that at the DB level too.
        builder.HasIndex(r => r.Top100GameSessionId).IsUnique();
    }
}

public class Top100GuessConfiguration : IEntityTypeConfiguration<Top100Guess>
{
    public void Configure(EntityTypeBuilder<Top100Guess> builder)
    {
        builder.ToTable("Top100Guesses");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.SequenceNumber).IsRequired();
        builder.Property(g => g.TeamId).IsRequired();
        builder.Property(g => g.GuessText).IsRequired().HasMaxLength(200);
        builder.Property(g => g.MatchedItemId);

        builder.HasIndex(g => new { g.Top100RoundId, g.SequenceNumber }).IsUnique();
    }
}
