using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class PasswordWordConfiguration : IEntityTypeConfiguration<PasswordWord>
{
    public void Configure(EntityTypeBuilder<PasswordWord> builder)
    {
        builder.ToTable("PasswordWords");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Word).IsRequired().HasMaxLength(200);

        builder.HasIndex(w => w.PasswordCategoryId);
    }
}
