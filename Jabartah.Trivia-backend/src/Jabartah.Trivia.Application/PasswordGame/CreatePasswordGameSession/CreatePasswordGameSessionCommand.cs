using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.CreatePasswordGameSession;

public record CreatePasswordGameSessionCommand(List<string> TeamNames, List<Guid> CategoryIds) : ICommand<CreatePasswordGameSessionResult>;

public record CreatePasswordGameSessionResult(Guid PasswordGameSessionId, List<PasswordTeamDto> Teams);
public record PasswordTeamDto(Guid Id, string Name, int Score);

public class CreatePasswordGameSessionHandler(IApplicationDbContext db)
    : ICommandHandler<CreatePasswordGameSessionCommand, CreatePasswordGameSessionResult>
{
    public async Task<CreatePasswordGameSessionResult> Handle(CreatePasswordGameSessionCommand command, CancellationToken ct)
    {
        var required = PasswordGameSession.RoundsPerTeam * 2;
        var available = await db.PasswordWords.CountAsync(w => command.CategoryIds.Contains(w.PasswordCategoryId), ct);
        if (available < required)
            throw new InvalidOperationException($"الفئات المختارة لا تحتوي على عدد كافٍ من الكلمات (المطلوب {required} على الأقل).");

        var session = PasswordGameSession.Create(command.TeamNames, command.CategoryIds);
        session.Start();

        db.PasswordGameSessions.Add(session); // whole graph fresh -> no MarkAdded needed
        await db.SaveChangesAsync(ct);

        return new CreatePasswordGameSessionResult(
            session.Id,
            session.Teams.Select(t => new PasswordTeamDto(t.Id, t.Name, t.Score)).ToList()
        );
    }
}
