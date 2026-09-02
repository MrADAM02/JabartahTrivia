using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Auth.GetAccount;

public record GetAccountQuery(Guid UserId) : IQuery<AccountDto>;

public record AccountDto(string Name, string Email, int GamesPlayedCount);

public class GetAccountHandler(IApplicationDbContext db) : IQueryHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery query, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == query.UserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var gamesPlayedCount =
            await db.GameSessions.CountAsync(s => s.UserId == query.UserId, ct)
            + await db.PasswordGameSessions.CountAsync(s => s.UserId == query.UserId, ct)
            + await db.RankingGameSessions.CountAsync(s => s.UserId == query.UserId, ct)
            + await db.Top100GameSessions.CountAsync(s => s.UserId == query.UserId, ct);

        return new AccountDto(user.Name, user.Email, gamesPlayedCount);
    }
}
