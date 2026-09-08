using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Words.ListWordsByCategory;

public record ListWordsByCategoryQuery(Guid CategoryId) : IQuery<List<AdminWordDto>>;

public record AdminWordDto(Guid Id, string Word);

public class ListWordsByCategoryHandler(IApplicationDbContext db) : IQueryHandler<ListWordsByCategoryQuery, List<AdminWordDto>>
{
    public async Task<List<AdminWordDto>> Handle(ListWordsByCategoryQuery query, CancellationToken ct) =>
        await db.PasswordWords
            .Where(w => w.PasswordCategoryId == query.CategoryId)
            .OrderBy(w => w.Word)
            .Select(w => new AdminWordDto(w.Id, w.Word))
            .ToListAsync(ct);
}
