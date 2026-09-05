using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Application.Top100Game.GetSession;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.EndGame;

public record EndTop100GameSessionCommand(Guid Top100GameSessionId) : ICommand<Top100SessionDto>;

public class EndTop100GameSessionHandler(IApplicationDbContext db) : ICommandHandler<EndTop100GameSessionCommand, Top100SessionDto>
{
    public async Task<Top100SessionDto> Handle(EndTop100GameSessionCommand command, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams)
            .FirstOrDefaultAsync(s => s.Id == command.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        session.Complete();
        await db.SaveChangesAsync(ct);

        // Ending early discards any in-progress round -- no points were awarded for it anyway.
        return new Top100SessionDto(
            session.Id,
            session.Status.ToString(),
            session.GuessesPerTeam,
            session.Teams.Select(t => new Top100TeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon)).ToList(),
            null,
            null
        );
    }
}
