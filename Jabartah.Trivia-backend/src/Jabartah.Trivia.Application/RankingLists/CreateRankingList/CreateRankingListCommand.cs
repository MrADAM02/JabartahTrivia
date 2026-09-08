using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.CreateRankingList;

public record CreateRankingListCommand(Guid CategoryId, string Title) : ICommand<CreateRankingListResult>;

public record CreateRankingListResult(Guid ListId);

public class CreateRankingListHandler(IApplicationDbContext db) : ICommandHandler<CreateRankingListCommand, CreateRankingListResult>
{
    public async Task<CreateRankingListResult> Handle(CreateRankingListCommand command, CancellationToken ct)
    {
        if (!await db.RankingCategories.AnyAsync(c => c.Id == command.CategoryId, ct))
            throw new KeyNotFoundException("Category not found.");

        var list = RankingList.Create(command.CategoryId, command.Title);
        db.RankingLists.Add(list);
        await db.SaveChangesAsync(ct);
        return new CreateRankingListResult(list.Id);
    }
}
