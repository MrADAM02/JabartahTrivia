using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.GetMyCategory;

public record GetMyCategoryQuery(Guid UserId, Guid CategoryId) : IQuery<MyCategoryDetailDto>;

public record MyCategoryQuestionDto(Guid Id, int PointValue, string Prompt, string Answer);
public record MyCategoryDetailDto(Guid Id, string Name, string? Icon, List<MyCategoryQuestionDto> Questions);

public class GetMyCategoryHandler(IApplicationDbContext db) : IQueryHandler<GetMyCategoryQuery, MyCategoryDetailDto>
{
    public async Task<MyCategoryDetailDto> Handle(GetMyCategoryQuery query, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == query.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        // Never let a user probe another user's custom category by guessing its GUID.
        if (category.OwnerUserId != query.UserId)
            throw new KeyNotFoundException("Category not found.");

        var questions = await db.Questions
            .Where(q => q.CategoryId == query.CategoryId)
            .OrderBy(q => q.PointValue)
            .Select(q => new MyCategoryQuestionDto(q.Id, q.PointValue, q.Prompt, q.Answer))
            .ToListAsync(ct);

        return new MyCategoryDetailDto(category.Id, category.Name, category.Icon, questions);
    }
}
