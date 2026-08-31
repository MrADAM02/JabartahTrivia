using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class PasswordTeamConfiguration : IEntityTypeConfiguration<PasswordTeam>
{
    public void Configure(EntityTypeBuilder<PasswordTeam> builder)
    {
        builder.ToTable("PasswordTeams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Score).IsRequired();
        builder.Property(t => t.TurnOrder).IsRequired();
        builder.HasIndex(t => t.PasswordGameSessionId);
    }
}
