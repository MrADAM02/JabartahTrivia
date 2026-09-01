using Jabartah.Trivia.Domain.GameSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("GameSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CompletedAt);

        // GameSession.Teams / .QuestionStates are backed by private List<T> fields
        // (_teams / _questionStates) with only an IReadOnlyCollection<T> exposed publicly.
        // EF Core's default backing-field convention picks these up automatically by name,
        // but we're explicit here so it's obvious why there's no public setter.
        builder.HasMany(s => s.Teams)
            .WithOne()
            .HasForeignKey(t => t.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Teams).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.QuestionStates)
            .WithOne()
            .HasForeignKey(q => q.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.QuestionStates).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Selected category IDs for this session -- stored as a Postgres uuid[] array
        // via EF Core's primitive collection support (EF Core 8+).
        builder.PrimitiveCollection(s => s.CategoryIds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class GameQuestionStateConfiguration : IEntityTypeConfiguration<GameQuestionState>
{
    public void Configure(EntityTypeBuilder<GameQuestionState> builder)
    {
        builder.ToTable("GameQuestionStates");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.RevealedAt).IsRequired();
        builder.Property(s => s.PowerUpTeamId);
        builder.Property(s => s.ActivePowerUp).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.AttemptFailed).IsRequired();
        builder.Property(s => s.IsResolved).IsRequired();
        builder.HasIndex(s => new { s.GameSessionId, s.QuestionId }).IsUnique();
    }
}
