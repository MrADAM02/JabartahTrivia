using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class RankingListItemConfiguration : IEntityTypeConfiguration<RankingListItem>
{
    public void Configure(EntityTypeBuilder<RankingListItem> builder)
    {
        builder.ToTable("RankingListItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Label).IsRequired().HasMaxLength(300);
        builder.Property(i => i.CorrectPosition).IsRequired();

        builder.HasIndex(i => i.RankingListId);
        builder.HasIndex(i => new { i.RankingListId, i.CorrectPosition }).IsUnique();
    }
}
