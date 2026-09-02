using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;

public record CreateTop100GameSessionCommand(List<string> TeamNames, List<Guid> CategoryIds, int RoundsPerTeam) : ICommand<CreateTop100GameSessionResult>;

public record CreateTop100GameSessionResult(Guid Top100GameSessionId, List<Top100TeamDto> Teams);
public record Top100TeamDto(Guid Id, string Name, int Score);

public class CreateTop100GameSessionHandler(IApplicationDbContext db, ICurrentUserAccessor currentUser)
    : ICommandHandler<CreateTop100GameSessionCommand, CreateTop100GameSessionResult>
{
    public async Task<CreateTop100GameSessionResult> Handle(CreateTop100GameSessionCommand command, CancellationToken ct)
    {
        var session = Top100GameSession.Create(command.TeamNames, command.CategoryIds, command.RoundsPerTeam); // throws first on bad input

        var required = session.RoundsPerTeam * 2; // each round consumes one LIST, not one item
        var available = await db.Top100Lists.CountAsync(l => session.CategoryIds.Contains(l.Top100CategoryId), ct);
        if (available < required)
            throw new InvalidOperationException($"الفئات المختارة لا تحتوي على عدد كافٍ من القوائم (المطلوب {required} على الأقل).");

        session.AttachOwner(currentUser.UserId); // null for guest play -- endpoint has no auth requirement
        session.Start();
        db.Top100GameSessions.Add(session); // whole graph fresh -> no MarkAdded needed
        await db.SaveChangesAsync(ct);

        return new CreateTop100GameSessionResult(
            session.Id,
            session.Teams.Select(t => new Top100TeamDto(t.Id, t.Name, t.Score)).ToList()
        );
    }
}
