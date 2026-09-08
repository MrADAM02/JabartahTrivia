using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.UpdateTop100Category;

public record UpdateTop100CategoryCommand(Guid CategoryId, string Name, string? Icon, string? Description) : ICommand<bool>;

public class UpdateTop100CategoryHandler(IApplicationDbContext db) : ICommandHandler<UpdateTop100CategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateTop100CategoryCommand command, CancellationToken ct)
    {
        var category = await db.Top100Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        category.UpdateDetails(command.Name, command.Icon, command.Description);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
