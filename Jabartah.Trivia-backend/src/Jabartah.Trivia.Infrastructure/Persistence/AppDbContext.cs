using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Categories;
using Jabartah.Trivia.Domain.GameSessions;
using Jabartah.Trivia.Domain.PasswordGame;
using Jabartah.Trivia.Domain.Questions;
using Jabartah.Trivia.Domain.RankingGame;
using Jabartah.Trivia.Domain.Teams;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Team> Teams => Set<Team>();

    public DbSet<PasswordCategory> PasswordCategories => Set<PasswordCategory>();
    public DbSet<PasswordWord> PasswordWords => Set<PasswordWord>();
    public DbSet<PasswordGameSession> PasswordGameSessions => Set<PasswordGameSession>();
    public DbSet<PasswordTeam> PasswordTeams => Set<PasswordTeam>();
    public DbSet<PasswordRevealToken> PasswordRevealTokens => Set<PasswordRevealToken>();

    public DbSet<RankingCategory> RankingCategories => Set<RankingCategory>();
    public DbSet<RankingList> RankingLists => Set<RankingList>();
    public DbSet<RankingListItem> RankingListItems => Set<RankingListItem>();
    public DbSet<RankingGameSession> RankingGameSessions => Set<RankingGameSession>();
    public DbSet<RankingTeam> RankingTeams => Set<RankingTeam>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public void MarkAdded<TEntity>(TEntity entity) where TEntity : class => Entry(entity).State = EntityState.Added;

    public override Task<int> SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}
