using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Categories;
using Jabartah.Trivia.Domain.Questions;

namespace Jabartah.Trivia.Application.Categories.CreateCustomCategory;

public record CustomQuestionInput(int PointValue, string Prompt, string Answer);

public record CreateCustomCategoryCommand(
    Guid UserId,
    string Name,
    string? Icon,
    List<CustomQuestionInput> Questions
) : ICommand<CreateCustomCategoryResult>;

public record CreateCustomCategoryResult(Guid CategoryId);

public class CreateCustomCategoryHandler(IApplicationDbContext db) : ICommandHandler<CreateCustomCategoryCommand, CreateCustomCategoryResult>
{
    private static readonly int[] RequiredTiers = [100, 200, 300, 400, 500];

    public async Task<CreateCustomCategoryResult> Handle(CreateCustomCategoryCommand command, CancellationToken ct)
    {
        if (command.Questions.Select(q => q.PointValue).OrderBy(p => p).SequenceEqual(RequiredTiers) is false)
            throw new ArgumentException("يجب إضافة سؤال واحد بالضبط لكل مستوى نقاط: 100، 200، 300، 400، 500.");

        var category = Category.Create(command.Name, command.Icon, command.UserId);
        db.Categories.Add(category);

        foreach (var q in command.Questions)
            db.Questions.Add(Question.Create(category.Id, q.PointValue, q.Prompt, q.Answer));

        await db.SaveChangesAsync(ct);
        return new CreateCustomCategoryResult(category.Id);
    }
}
