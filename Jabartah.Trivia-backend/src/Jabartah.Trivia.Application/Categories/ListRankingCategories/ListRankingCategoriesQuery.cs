using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListRankingCategories;

public record ListRankingCategoriesQuery : IQuery<List<RankingCategoryDto>>;

public record RankingCategoryDto(Guid Id, string Name, string? Icon);

public class ListRankingCategoriesHandler(IApplicationDbContext db) : IQueryHandler<ListRankingCategoriesQuery, List<RankingCategoryDto>>
{
    public async Task<List<RankingCategoryDto>> Handle(ListRankingCategoriesQuery query, CancellationToken ct) =>
        await db.RankingCategories
            .OrderBy(c => c.Name)
            .Select(c => new RankingCategoryDto(c.Id, c.Name, c.Icon))
            .ToListAsync(ct);
}
