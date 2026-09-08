using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Questions.ListQuestionsByCategory;

public record ListQuestionsByCategoryQuery(Guid CategoryId) : IQuery<List<AdminQuestionDto>>;

public record AdminQuestionDto(Guid Id, int PointValue, string Prompt, string Answer, string? MediaUrl);

public class ListQuestionsByCategoryHandler(IApplicationDbContext db) : IQueryHandler<ListQuestionsByCategoryQuery, List<AdminQuestionDto>>
{
    public async Task<List<AdminQuestionDto>> Handle(ListQuestionsByCategoryQuery query, CancellationToken ct) =>
        await db.Questions
            .Where(q => q.CategoryId == query.CategoryId)
            .OrderBy(q => q.PointValue)
            .Select(q => new AdminQuestionDto(q.Id, q.PointValue, q.Prompt, q.Answer, q.MediaUrl))
            .ToListAsync(ct);
}
