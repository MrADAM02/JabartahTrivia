using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.PasswordGame;

namespace Jabartah.Trivia.Application.Categories.CreatePasswordCategory;

public record CreatePasswordCategoryCommand(string Name, string? Icon) : ICommand<CreatePasswordCategoryResult>;

public record CreatePasswordCategoryResult(Guid CategoryId);

public class CreatePasswordCategoryHandler(IApplicationDbContext db) : ICommandHandler<CreatePasswordCategoryCommand, CreatePasswordCategoryResult>
{
    public async Task<CreatePasswordCategoryResult> Handle(CreatePasswordCategoryCommand command, CancellationToken ct)
    {
        var category = PasswordCategory.Create(command.Name, command.Icon);
        db.PasswordCategories.Add(category);
        await db.SaveChangesAsync(ct);
        return new CreatePasswordCategoryResult(category.Id);
    }
}
