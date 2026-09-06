using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.GameSessions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.CreateGameSession;

public record CreateGameSessionCommand(
    List<TeamSetupInput> Teams,
    List<Guid> CategoryIds
) : ICommand<CreateGameSessionResult>;

public record CreateGameSessionResult(Guid GameSessionId, List<TeamDto> Teams);
public record TeamDto(Guid Id, string Name, int Score, bool DoublePointsAvailable, bool TwoAnswersAvailable, string? Color, string? Icon, bool HalfOpponentTimerAvailable);

public class CreateGameSessionHandler(IApplicationDbContext db, ICurrentUserAccessor currentUser)
    : ICommandHandler<CreateGameSessionCommand, CreateGameSessionResult>
{
    public async Task<CreateGameSessionResult> Handle(CreateGameSessionCommand command, CancellationToken ct)
    {
        // A category can now hold several candidate questions per point value (up to 5) --
        // pick exactly one per (category, point value) here, once, so the board stays stable
        // for this session's lifetime instead of re-randomizing on every GetBoard call.
        var candidates = await db.Questions
            .Where(q => command.CategoryIds.Contains(q.CategoryId))
            .Select(q => new { q.Id, q.CategoryId, q.PointValue })
            .ToListAsync(ct);

        var boardQuestionIds = new List<Guid>();
        foreach (var categoryId in command.CategoryIds)
        {
            foreach (var pointValue in new[] { 100, 200, 300, 400, 500 })
            {
                var pool = candidates.Where(q => q.CategoryId == categoryId && q.PointValue == pointValue).ToList();
                if (pool.Count == 0)
                    throw new InvalidOperationException("إحدى الفئات المختارة لا تحتوي على سؤال لأحد مستويات النقاط.");
                boardQuestionIds.Add(pool[Random.Shared.Next(pool.Count)].Id);
            }
        }

        var session = GameSession.Create(command.Teams.Select(t => (t.Name, t.Color, t.Icon)), command.CategoryIds, boardQuestionIds);
        session.AttachOwner(currentUser.UserId); // null for guest play -- endpoint has no auth requirement
        session.Start(); // MVP: no separate "waiting room" step, start immediately

        db.GameSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return new CreateGameSessionResult(
            session.Id,
            session.Teams.Select(t => new TeamDto(t.Id, t.Name, t.Score, t.DoublePointsAvailable, t.TwoAnswersAvailable, t.Color, t.Icon, t.HalfOpponentTimerAvailable)).ToList()
        );
    }
}
