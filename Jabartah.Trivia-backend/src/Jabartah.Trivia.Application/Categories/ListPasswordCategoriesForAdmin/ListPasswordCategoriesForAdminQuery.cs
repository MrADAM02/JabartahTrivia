using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListPasswordCategoriesForAdmin;

public record ListPasswordCategoriesForAdminQuery : IQuery<List<AdminPasswordCategoryDto>>;

public record AdminPasswordCategoryDto(Guid Id, string Name, string? Icon, int WordCount);

public class ListPasswordCategoriesForAdminHandler(IApplicationDbContext db)
    : IQueryHandler<ListPasswordCategoriesForAdminQuery, List<AdminPasswordCategoryDto>>
{
    public async Task<List<AdminPasswordCategoryDto>> Handle(ListPasswordCategoriesForAdminQuery query, CancellationToken ct) =>
        await db.PasswordCategories
            .OrderBy(c => c.Name)
            .Select(c => new AdminPasswordCategoryDto(c.Id, c.Name, c.Icon, db.PasswordWords.Count(w => w.PasswordCategoryId == c.Id)))
            .ToListAsync(ct);
}
