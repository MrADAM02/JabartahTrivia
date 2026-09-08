using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Categories;

namespace Jabartah.Trivia.Application.Categories.CreateCategory;

public record CreateCategoryCommand(string Name, string? Icon) : ICommand<CreateCategoryResult>;

public record CreateCategoryResult(Guid CategoryId);

// Admin-authored category: always global (OwnerUserId = null), same as seeded ones --
// distinct from CreateCustomCategoryCommand, which creates a user's own تصنيفاتي category
// and requires exactly one question per point tier up front.
public class CreateCategoryHandler(IApplicationDbContext db) : ICommandHandler<CreateCategoryCommand, CreateCategoryResult>
{
    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var category = Category.Create(command.Name, command.Icon);
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return new CreateCategoryResult(category.Id);
    }
}
