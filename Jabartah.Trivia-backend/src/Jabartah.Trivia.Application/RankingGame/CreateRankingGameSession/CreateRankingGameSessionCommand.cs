using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingGame.CreateRankingGameSession;

public record CreateRankingGameSessionCommand(List<string> TeamNames, List<Guid> CategoryIds, int RoundsPerTeam) : ICommand<CreateRankingGameSessionResult>;

public record CreateRankingGameSessionResult(Guid RankingGameSessionId, List<RankingTeamDto> Teams);
public record RankingTeamDto(Guid Id, string Name, int Score);

public class CreateRankingGameSessionHandler(IApplicationDbContext db, ICurrentUserAccessor currentUser)
    : ICommandHandler<CreateRankingGameSessionCommand, CreateRankingGameSessionResult>
{
    public async Task<CreateRankingGameSessionResult> Handle(CreateRankingGameSessionCommand command, CancellationToken ct)
    {
        var session = RankingGameSession.Create(command.TeamNames, command.CategoryIds, command.RoundsPerTeam); // throws first on bad input

        var required = session.RoundsPerTeam * 2;
        var available = await db.RankingLists.CountAsync(l => session.CategoryIds.Contains(l.RankingCategoryId), ct);
        if (available < required)
            throw new InvalidOperationException($"الفئات المختارة لا تحتوي على عدد كافٍ من القوائم (المطلوب {required} على الأقل).");

        session.AttachOwner(currentUser.UserId); // null for guest play -- endpoint has no auth requirement
        session.Start();

        db.RankingGameSessions.Add(session); // whole graph fresh -> no MarkAdded needed
        await db.SaveChangesAsync(ct);

        return new CreateRankingGameSessionResult(
            session.Id,
            session.Teams.Select(t => new RankingTeamDto(t.Id, t.Name, t.Score)).ToList()
        );
    }
}
