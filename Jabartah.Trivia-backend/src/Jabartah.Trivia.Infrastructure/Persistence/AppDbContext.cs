using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Categories;
using Jabartah.Trivia.Domain.GameSessions;
using Jabartah.Trivia.Domain.Questions;
using Jabartah.Trivia.Domain.Teams;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public void MarkAdded<TEntity>(TEntity entity) where TEntity : class => Entry(entity).State = EntityState.Added;

    public override Task<int> SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}
