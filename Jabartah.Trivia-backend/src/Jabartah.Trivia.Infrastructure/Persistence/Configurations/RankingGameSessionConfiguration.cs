using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class RankingGameSessionConfiguration : IEntityTypeConfiguration<RankingGameSession>
{
    public void Configure(EntityTypeBuilder<RankingGameSession> builder)
    {
        builder.ToTable("RankingGameSessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CompletedAt);

        builder.HasMany(s => s.Teams)
            .WithOne()
            .HasForeignKey(t => t.RankingGameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Teams).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Rounds)
            .WithOne()
            .HasForeignKey(r => r.RankingGameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Rounds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.PrimitiveCollection(s => s.CategoryIds).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class RankingRoundConfiguration : IEntityTypeConfiguration<RankingRound>
{
    public void Configure(EntityTypeBuilder<RankingRound> builder)
    {
        builder.ToTable("RankingRounds");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.PointsAwarded).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ResolvedAt);

        builder.HasIndex(r => new { r.RankingGameSessionId, r.RoundNumber }).IsUnique();
    }
}
