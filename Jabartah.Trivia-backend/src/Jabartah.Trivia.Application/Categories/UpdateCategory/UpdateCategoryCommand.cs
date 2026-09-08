using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.UpdateCategory;

public record UpdateCategoryCommand(Guid CategoryId, string Name, string? Icon) : ICommand<bool>;

public class UpdateCategoryHandler(IApplicationDbContext db) : ICommandHandler<UpdateCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        category.UpdateDetails(command.Name, command.Icon);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
