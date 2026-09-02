using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.GameSessions;

namespace Jabartah.Trivia.Application.GameSessions.CreateGameSession;

public record CreateGameSessionCommand(
    List<TeamSetupInput> Teams,
    List<Guid> CategoryIds
) : ICommand<CreateGameSessionResult>;

public record CreateGameSessionResult(Guid GameSessionId, List<TeamDto> Teams);
public record TeamDto(Guid Id, string Name, int Score, bool DoublePointsAvailable, bool TwoAnswersAvailable, string? Color, string? Icon);

public class CreateGameSessionHandler(IApplicationDbContext db, ICurrentUserAccessor currentUser)
    : ICommandHandler<CreateGameSessionCommand, CreateGameSessionResult>
{
    public async Task<CreateGameSessionResult> Handle(CreateGameSessionCommand command, CancellationToken ct)
    {
        var session = GameSession.Create(command.Teams.Select(t => (t.Name, t.Color, t.Icon)), command.CategoryIds);
        session.AttachOwner(currentUser.UserId); // null for guest play -- endpoint has no auth requirement
        session.Start(); // MVP: no separate "waiting room" step, start immediately

        db.GameSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return new CreateGameSessionResult(
            session.Id,
            session.Teams.Select(t => new TeamDto(t.Id, t.Name, t.Score, t.DoublePointsAvailable, t.TwoAnswersAvailable, t.Color, t.Icon)).ToList()
        );
    }
}
