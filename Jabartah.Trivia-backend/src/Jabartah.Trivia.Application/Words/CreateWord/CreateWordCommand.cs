using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Words.CreateWord;

public record CreateWordCommand(Guid CategoryId, string Word) : ICommand<CreateWordResult>;

public record CreateWordResult(Guid WordId);

public class CreateWordHandler(IApplicationDbContext db) : ICommandHandler<CreateWordCommand, CreateWordResult>
{
    public async Task<CreateWordResult> Handle(CreateWordCommand command, CancellationToken ct)
    {
        if (!await db.PasswordCategories.AnyAsync(c => c.Id == command.CategoryId, ct))
            throw new KeyNotFoundException("Category not found.");

        var word = PasswordWord.Create(command.CategoryId, command.Word);
        db.PasswordWords.Add(word);
        await db.SaveChangesAsync(ct);
        return new CreateWordResult(word.Id);
    }
}
