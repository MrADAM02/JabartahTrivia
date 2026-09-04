using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.UseExtraTime;

public record UseExtraTimeCommand(Guid PasswordGameSessionId, Guid TeamId) : ICommand<UseExtraTimeResult>;

public record UseExtraTimeResult(bool ExtraTimeAvailable);

public class UseExtraTimeHandler(IApplicationDbContext db) : ICommandHandler<UseExtraTimeCommand, UseExtraTimeResult>
{
    public async Task<UseExtraTimeResult> Handle(UseExtraTimeCommand command, CancellationToken ct)
    {
        var session = await db.PasswordGameSessions
            .Include(s => s.Teams)
            .FirstOrDefaultAsync(s => s.Id == command.PasswordGameSessionId, ct)
            ?? throw new KeyNotFoundException("Password game session not found.");

        session.UseExtraTime(command.TeamId); // throws if already used or team doesn't belong to this session
        await db.SaveChangesAsync(ct);

        return new UseExtraTimeResult(false);
    }
}
