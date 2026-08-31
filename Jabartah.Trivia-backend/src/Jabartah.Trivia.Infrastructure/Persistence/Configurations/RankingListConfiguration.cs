using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class RankingListConfiguration : IEntityTypeConfiguration<RankingList>
{
    public void Configure(EntityTypeBuilder<RankingList> builder)
    {
        builder.ToTable("RankingLists");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Title).IsRequired().HasMaxLength(300);

        builder.HasIndex(l => l.RankingCategoryId);
    }
}
