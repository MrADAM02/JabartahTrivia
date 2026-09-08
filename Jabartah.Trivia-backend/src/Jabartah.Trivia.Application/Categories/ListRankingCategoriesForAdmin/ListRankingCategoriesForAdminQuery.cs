using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListRankingCategoriesForAdmin;

public record ListRankingCategoriesForAdminQuery : IQuery<List<AdminRankingCategoryDto>>;

public record AdminRankingCategoryDto(Guid Id, string Name, string? Icon, int ListCount);

public class ListRankingCategoriesForAdminHandler(IApplicationDbContext db)
    : IQueryHandler<ListRankingCategoriesForAdminQuery, List<AdminRankingCategoryDto>>
{
    public async Task<List<AdminRankingCategoryDto>> Handle(ListRankingCategoriesForAdminQuery query, CancellationToken ct) =>
        await db.RankingCategories
            .OrderBy(c => c.Name)
            .Select(c => new AdminRankingCategoryDto(c.Id, c.Name, c.Icon, db.RankingLists.Count(l => l.RankingCategoryId == c.Id)))
            .ToListAsync(ct);
}
