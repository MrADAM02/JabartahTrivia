using Jabartah.Trivia.Domain.Categories;
using Jabartah.Trivia.Domain.GameSessions;
using Jabartah.Trivia.Domain.PasswordGame;
using Jabartah.Trivia.Domain.Questions;
using Jabartah.Trivia.Domain.RankingGame;
using Jabartah.Trivia.Domain.Teams;
using Jabartah.Trivia.Domain.Top100Game;
using Jabartah.Trivia.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Abstractions;

// Deliberately not wrapped in a repository-per-aggregate layer -- EF Core's DbSet
// already is a repository/unit-of-work. This interface just keeps Application
// from taking a hard dependency on the Infrastructure project's DbContext class.
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Category> Categories { get; }
    DbSet<Question> Questions { get; }
    DbSet<GameSession> GameSessions { get; }
    DbSet<Team> Teams { get; }

    DbSet<PasswordCategory> PasswordCategories { get; }
    DbSet<PasswordWord> PasswordWords { get; }
    DbSet<PasswordGameSession> PasswordGameSessions { get; }
    DbSet<PasswordTeam> PasswordTeams { get; }
    DbSet<PasswordRevealToken> PasswordRevealTokens { get; }

    DbSet<RankingCategory> RankingCategories { get; }
    DbSet<RankingList> RankingLists { get; }
    DbSet<RankingListItem> RankingListItems { get; }
    DbSet<RankingGameSession> RankingGameSessions { get; }
    DbSet<RankingTeam> RankingTeams { get; }

    DbSet<Top100Category> Top100Categories { get; }
    DbSet<Top100List> Top100Lists { get; }
    DbSet<Top100ListItem> Top100ListItems { get; }
    DbSet<Top100GameSession> Top100GameSessions { get; }
    DbSet<Top100Team> Top100Teams { get; }

    // EF Core can't tell "new" from "existing" for entities with a client-generated key
    // (e.g. GameQuestionState.Id set via Guid.NewGuid() in the domain) when they're attached
    // to an already-tracked aggregate purely by mutating a private collection field -- it
    // assumes Modified instead of Added. Call this right after creating such a child so
    // SaveChanges emits an INSERT instead of a no-op UPDATE.
    void MarkAdded<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct);
}
