using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListCategoriesForAdmin;

public record ListCategoriesForAdminQuery : IQuery<List<AdminCategoryDto>>;

public record AdminCategoryDto(Guid Id, string Name, string? Icon, int QuestionCount);

// Unlike ListCategoriesQuery (the public board picker), this intentionally still only
// shows global categories (OwnerUserId == null) -- users' own تصنيفاتي categories are
// their private content, not something an admin manages here.
public class ListCategoriesForAdminHandler(IApplicationDbContext db) : IQueryHandler<ListCategoriesForAdminQuery, List<AdminCategoryDto>>
{
    public async Task<List<AdminCategoryDto>> Handle(ListCategoriesForAdminQuery query, CancellationToken ct) =>
        await db.Categories
            .Where(c => c.OwnerUserId == null)
            .OrderBy(c => c.Name)
            .Select(c => new AdminCategoryDto(c.Id, c.Name, c.Icon, db.Questions.Count(q => q.CategoryId == c.Id)))
            .ToListAsync(ct);
}
