using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Words.DeleteWord;

public record DeleteWordCommand(Guid WordId) : ICommand<bool>;

// PasswordRound.PasswordWordId is a real FK-shaped reference to a played round's word --
// a word that was ever drawn in a game can't be removed without corrupting that history.
public class DeleteWordHandler(IApplicationDbContext db) : ICommandHandler<DeleteWordCommand, bool>
{
    public async Task<bool> Handle(DeleteWordCommand command, CancellationToken ct)
    {
        var word = await db.PasswordWords.FirstOrDefaultAsync(w => w.Id == command.WordId, ct)
            ?? throw new KeyNotFoundException("Word not found.");

        var isUsedInAnyRound = await db.PasswordGameSessions
            .SelectMany(s => s.Rounds)
            .AnyAsync(r => r.PasswordWordId == command.WordId, ct);

        if (isUsedInAnyRound)
            throw new InvalidOperationException("لا يمكن حذف كلمة تم استخدامها في لعبة سابقة.");

        db.PasswordWords.Remove(word);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
