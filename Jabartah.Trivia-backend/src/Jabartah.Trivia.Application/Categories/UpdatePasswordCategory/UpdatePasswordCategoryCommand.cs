using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.UpdatePasswordCategory;

public record UpdatePasswordCategoryCommand(Guid CategoryId, string Name, string? Icon) : ICommand<bool>;

public class UpdatePasswordCategoryHandler(IApplicationDbContext db) : ICommandHandler<UpdatePasswordCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdatePasswordCategoryCommand command, CancellationToken ct)
    {
        var category = await db.PasswordCategories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        category.UpdateDetails(command.Name, command.Icon);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
