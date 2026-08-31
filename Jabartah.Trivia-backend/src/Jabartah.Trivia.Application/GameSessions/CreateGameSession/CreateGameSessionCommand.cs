using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.GameSessions;

namespace Jabartah.Trivia.Application.GameSessions.CreateGameSession;

public record CreateGameSessionCommand(
    List<string> TeamNames,
    List<Guid> CategoryIds
) : ICommand<CreateGameSessionResult>;

public record CreateGameSessionResult(Guid GameSessionId, List<TeamDto> Teams);
public record TeamDto(Guid Id, string Name, int Score);

public class CreateGameSessionHandler(IApplicationDbContext db)
    : ICommandHandler<CreateGameSessionCommand, CreateGameSessionResult>
{
    public async Task<CreateGameSessionResult> Handle(CreateGameSessionCommand command, CancellationToken ct)
    {
        var session = GameSession.Create(command.TeamNames, command.CategoryIds);
        session.Start(); // MVP: no separate "waiting room" step, start immediately

        db.GameSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return new CreateGameSessionResult(
            session.Id,
            session.Teams.Select(t => new TeamDto(t.Id, t.Name, t.Score)).ToList()
        );
    }
}
