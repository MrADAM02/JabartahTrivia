using Jabartah.Trivia.Domain.Categories;
using Jabartah.Trivia.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(50);

        // A تصنيفاتي category has no stakeholder besides its creator -- deleting the
        // account deletes the category (and, via QuestionConfiguration, its questions).
        builder.HasOne<User>().WithMany().HasForeignKey(c => c.OwnerUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => c.OwnerUserId);
    }
}
