using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class RankingCategoryConfiguration : IEntityTypeConfiguration<RankingCategory>
{
    public void Configure(EntityTypeBuilder<RankingCategory> builder)
    {
        builder.ToTable("RankingCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(50);
    }
}
