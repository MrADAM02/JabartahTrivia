using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.CreateTop100List;

public record CreateTop100ListCommand(Guid CategoryId, string Title) : ICommand<CreateTop100ListResult>;

public record CreateTop100ListResult(Guid ListId);

public class CreateTop100ListHandler(IApplicationDbContext db) : ICommandHandler<CreateTop100ListCommand, CreateTop100ListResult>
{
    public async Task<CreateTop100ListResult> Handle(CreateTop100ListCommand command, CancellationToken ct)
    {
        if (!await db.Top100Categories.AnyAsync(c => c.Id == command.CategoryId, ct))
            throw new KeyNotFoundException("Category not found.");

        var list = Top100List.Create(command.CategoryId, command.Title);
        db.Top100Lists.Add(list);
        await db.SaveChangesAsync(ct);
        return new CreateTop100ListResult(list.Id);
    }
}
