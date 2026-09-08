using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListTop100CategoriesForAdmin;

public record ListTop100CategoriesForAdminQuery : IQuery<List<AdminTop100CategoryDto>>;

public record AdminTop100CategoryDto(Guid Id, string Name, string? Icon, string? Description, int ListCount);

public class ListTop100CategoriesForAdminHandler(IApplicationDbContext db)
    : IQueryHandler<ListTop100CategoriesForAdminQuery, List<AdminTop100CategoryDto>>
{
    public async Task<List<AdminTop100CategoryDto>> Handle(ListTop100CategoriesForAdminQuery query, CancellationToken ct) =>
        await db.Top100Categories
            .OrderBy(c => c.Name)
            .Select(c => new AdminTop100CategoryDto(c.Id, c.Name, c.Icon, c.Description, db.Top100Lists.Count(l => l.Top100CategoryId == c.Id)))
            .ToListAsync(ct);
}
