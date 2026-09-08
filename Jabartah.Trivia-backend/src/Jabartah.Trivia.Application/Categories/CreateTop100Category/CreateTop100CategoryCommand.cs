using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Top100Game;

namespace Jabartah.Trivia.Application.Categories.CreateTop100Category;

public record CreateTop100CategoryCommand(string Name, string? Icon, string? Description) : ICommand<CreateTop100CategoryResult>;

public record CreateTop100CategoryResult(Guid CategoryId);

public class CreateTop100CategoryHandler(IApplicationDbContext db) : ICommandHandler<CreateTop100CategoryCommand, CreateTop100CategoryResult>
{
    public async Task<CreateTop100CategoryResult> Handle(CreateTop100CategoryCommand command, CancellationToken ct)
    {
        var category = Top100Category.Create(command.Name, command.Icon, command.Description);
        db.Top100Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return new CreateTop100CategoryResult(category.Id);
    }
}
