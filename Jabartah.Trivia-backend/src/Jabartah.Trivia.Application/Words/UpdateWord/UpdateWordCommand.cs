using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Words.UpdateWord;

public record UpdateWordCommand(Guid WordId, string Word) : ICommand<bool>;

public class UpdateWordHandler(IApplicationDbContext db) : ICommandHandler<UpdateWordCommand, bool>
{
    public async Task<bool> Handle(UpdateWordCommand command, CancellationToken ct)
    {
        var word = await db.PasswordWords.FirstOrDefaultAsync(w => w.Id == command.WordId, ct)
            ?? throw new KeyNotFoundException("Word not found.");

        word.UpdateDetails(command.Word);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
