using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.UpdateRankingCategory;

public record UpdateRankingCategoryCommand(Guid CategoryId, string Name, string? Icon) : ICommand<bool>;

public class UpdateRankingCategoryHandler(IApplicationDbContext db) : ICommandHandler<UpdateRankingCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateRankingCategoryCommand command, CancellationToken ct)
    {
        var category = await db.RankingCategories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        category.UpdateDetails(command.Name, command.Icon);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
