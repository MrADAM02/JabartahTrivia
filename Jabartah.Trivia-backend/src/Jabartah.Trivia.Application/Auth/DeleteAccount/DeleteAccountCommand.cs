using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Auth.DeleteAccount;

public record DeleteAccountCommand(Guid UserId) : ICommand<bool>;

public class DeleteAccountHandler(IApplicationDbContext db) : ICommandHandler<DeleteAccountCommand, bool>
{
    public async Task<bool> Handle(DeleteAccountCommand command, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        // Deleting the row cascades at the DB level per the FK configuration:
        // owned game sessions are anonymized (UserId -> null, SetNull), owned
        // تصنيفاتي categories/questions are deleted (Cascade).
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
