using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.RankingGame;

namespace Jabartah.Trivia.Application.Categories.CreateRankingCategory;

public record CreateRankingCategoryCommand(string Name, string? Icon) : ICommand<CreateRankingCategoryResult>;

public record CreateRankingCategoryResult(Guid CategoryId);

public class CreateRankingCategoryHandler(IApplicationDbContext db) : ICommandHandler<CreateRankingCategoryCommand, CreateRankingCategoryResult>
{
    public async Task<CreateRankingCategoryResult> Handle(CreateRankingCategoryCommand command, CancellationToken ct)
    {
        var category = RankingCategory.Create(command.Name, command.Icon);
        db.RankingCategories.Add(category);
        await db.SaveChangesAsync(ct);
        return new CreateRankingCategoryResult(category.Id);
    }
}
