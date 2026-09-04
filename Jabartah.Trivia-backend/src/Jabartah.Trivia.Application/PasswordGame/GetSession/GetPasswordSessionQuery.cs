using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.PasswordGame.CreatePasswordGameSession;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.GetSession;

public record GetPasswordSessionQuery(Guid PasswordGameSessionId) : IQuery<PasswordSessionDto>;

public record PasswordSessionDto(Guid Id, string Status, List<PasswordTeamDto> Teams, int RoundsPlayed, int TotalRounds, PasswordPendingRoundDto? PendingRound);
public record PasswordPendingRoundDto(Guid RoundId, Guid TeamId, string TeamName, int RoundNumber);

public class GetPasswordSessionHandler(IApplicationDbContext db) : IQueryHandler<GetPasswordSessionQuery, PasswordSessionDto>
{
    public async Task<PasswordSessionDto> Handle(GetPasswordSessionQuery query, CancellationToken ct)
    {
        var session = await db.PasswordGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == query.PasswordGameSessionId, ct)
            ?? throw new KeyNotFoundException("Password game session not found.");

        var pending = session.Rounds.FirstOrDefault(r => r.Outcome == PasswordRoundOutcome.Pending);
        PasswordPendingRoundDto? pendingDto = pending is null
            ? null
            : new PasswordPendingRoundDto(pending.Id, pending.TeamId, session.Teams.First(t => t.Id == pending.TeamId).Name, pending.RoundNumber);

        return new PasswordSessionDto(
            session.Id,
            session.Status.ToString(),
            session.Teams.Select(t => new PasswordTeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon, t.ExtraTimeAvailable)).ToList(),
            session.Rounds.Count(r => r.Outcome != PasswordRoundOutcome.Pending),
            session.MaxRounds,
            pendingDto
        );
    }
}
