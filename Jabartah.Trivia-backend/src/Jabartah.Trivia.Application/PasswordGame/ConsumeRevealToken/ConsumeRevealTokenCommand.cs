using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.ConsumeRevealToken;

public record ConsumeRevealTokenCommand(string Token) : ICommand<ConsumeRevealTokenResult>;

public record ConsumeRevealTokenResult(bool Success, bool Expired, bool AlreadyConsumed, string? Word, string? CategoryName);

public class ConsumeRevealTokenHandler(IApplicationDbContext db) : ICommandHandler<ConsumeRevealTokenCommand, ConsumeRevealTokenResult>
{
    public async Task<ConsumeRevealTokenResult> Handle(ConsumeRevealTokenCommand command, CancellationToken ct)
    {
        var token = await db.PasswordRevealTokens.FirstOrDefaultAsync(t => t.Token == command.Token, ct)
            ?? throw new KeyNotFoundException("Reveal link not found.");

        if (!token.TryConsume(out var expired, out var alreadyConsumed))
            return new ConsumeRevealTokenResult(false, expired, alreadyConsumed, null, null); // nothing mutated, no SaveChanges

        var word = await db.PasswordWords.FirstAsync(w => w.Id == token.PasswordWordId, ct);
        var category = await db.PasswordCategories.FirstAsync(c => c.Id == word.PasswordCategoryId, ct);

        await db.SaveChangesAsync(ct); // persists token.ConsumedAt (plain UPDATE, no MarkAdded)
        return new ConsumeRevealTokenResult(true, false, false, word.Word, category.Name);
    }
}
