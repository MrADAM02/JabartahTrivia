using Jabartah.Trivia.Domain.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jabartah.Trivia.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Prompt).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.Answer).IsRequired().HasMaxLength(500);
        builder.Property(q => q.MediaUrl).HasMaxLength(500);
        builder.Property(q => q.PointValue).IsRequired();

        builder.HasIndex(q => q.CategoryId);
    }
}
