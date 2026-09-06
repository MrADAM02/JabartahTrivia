using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.GetBoard;

public record GetBoardQuery(Guid GameSessionId) : IQuery<BoardDto>;

public record BoardDto(
    Guid GameSessionId,
    List<TeamDto> Teams,
    List<CategoryColumnDto> Categories,
    Guid CurrentTurnTeamId,
    string CurrentTurnTeamName,
    Guid? PendingTimerDebuffTeamId
);
public record CategoryColumnDto(Guid CategoryId, string Name, string? Icon, List<BoardCellDto> Cells);
public record BoardCellDto(Guid QuestionId, int PointValue, bool IsRevealed, Guid? WonByTeamId);

public class GetBoardHandler(IApplicationDbContext db) : IQueryHandler<GetBoardQuery, BoardDto>
{
    public async Task<BoardDto> Handle(GetBoardQuery query, CancellationToken ct)
    {
        var session = await db.GameSessions
            .Include(s => s.Teams)
            .Include(s => s.QuestionStates)
            .FirstOrDefaultAsync(s => s.Id == query.GameSessionId, ct)
            ?? throw new KeyNotFoundException("Game session not found.");

        var categories = await db.Categories
            .Where(c => session.CategoryIds.Contains(c.Id))
            .ToListAsync(ct);

        // Filtered to this session's own persisted picks (BoardQuestionIds) -- a category can
        // have several candidate questions per point value in the DB, but exactly one was
        // chosen per (category, point value) at session-creation time, so this always yields
        // exactly 5 cells per category regardless of how many candidates exist.
        var questions = await db.Questions
            .Where(q => session.CategoryIds.Contains(q.CategoryId) && session.BoardQuestionIds.Contains(q.Id))
            .ToListAsync(ct);

        var stateByQuestionId = session.QuestionStates.ToDictionary(s => s.QuestionId);

        var columns = categories.Select(category => new CategoryColumnDto(
            category.Id,
            category.Name,
            category.Icon,
            questions
                .Where(q => q.CategoryId == category.Id)
                .OrderBy(q => q.PointValue)
                .Select(q =>
                {
                    stateByQuestionId.TryGetValue(q.Id, out var state);
                    return new BoardCellDto(q.Id, q.PointValue, state is not null, state?.WonByTeamId);
                })
                .ToList()
        )).ToList();

        var turnTeam = session.Teams.First(t => t.Id == session.CurrentTurnTeamId);

        return new BoardDto(
            session.Id,
            session.Teams.Select(t => new TeamDto(t.Id, t.Name, t.Score, t.DoublePointsAvailable, t.TwoAnswersAvailable, t.Color, t.Icon, t.HalfOpponentTimerAvailable)).ToList(),
            columns,
            turnTeam.Id,
            turnTeam.Name,
            session.PendingTimerDebuffTeamId
        );
    }
}
