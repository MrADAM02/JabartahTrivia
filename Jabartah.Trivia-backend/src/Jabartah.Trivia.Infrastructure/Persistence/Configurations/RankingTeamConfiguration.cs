using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class RankingTeamConfiguration : IEntityTypeConfiguration<RankingTeam>
{
    public void Configure(EntityTypeBuilder<RankingTeam> builder)
    {
        builder.ToTable("RankingTeams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Score).IsRequired();
        builder.Property(t => t.TurnOrder).IsRequired();
        builder.Property(t => t.Color).HasMaxLength(20);
        builder.Property(t => t.Icon).HasMaxLength(50);
        builder.Property(t => t.RevealPositionAvailable).IsRequired();
        builder.HasIndex(t => t.RankingGameSessionId);
    }
}
