namespace Jabartah.Trivia.Domain.RankingGame;

public enum RankingGameSessionStatus
{
    Setup = 0,
    InProgress = 1,
    Completed = 2
}

public enum RankingRoundStatus
{
    Pending = 0,
    Submitted = 1
}

// Tracks one played round: which team is ranking, which list, and the resulting score.
public class RankingRound
{
    public Guid Id { get; private set; }
    public Guid RankingGameSessionId { get; private set; }
    public int RoundNumber { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid RankingListId { get; private set; }
    public RankingRoundStatus Status { get; private set; }
    public int PointsAwarded { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private RankingRound() { } // EF Core

    public static RankingRound Create(Guid sessionId, int roundNumber, Guid teamId, Guid listId) =>
        new()
        {
            Id = Guid.NewGuid(),
            RankingGameSessionId = sessionId,
            RoundNumber = roundNumber,
            TeamId = teamId,
            RankingListId = listId,
            Status = RankingRoundStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

    public void Submit(int pointsAwarded)
    {
        if (Status != RankingRoundStatus.Pending)
            throw new InvalidOperationException("This round was already submitted.");
        PointsAwarded = pointsAwarded;
        Status = RankingRoundStatus.Submitted;
        ResolvedAt = DateTime.UtcNow;
    }
}

// Aggregate root: two teams alternate turns arranging shuffled cards into the correct order.
public class RankingGameSession
{
    public static readonly int[] AllowedRoundsPerTeam = [2, 4, 6];

    public int RoundsPerTeam { get; private set; }

    public Guid Id { get; private set; }
    public RankingGameSessionStatus Status { get; private set; }
    public Guid? UserId { get; private set; }   // owner, if created while logged in; null for guest play
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<RankingTeam> _teams = new();
    public IReadOnlyCollection<RankingTeam> Teams => _teams.AsReadOnly();

    private readonly List<Guid> _categoryIds = new();
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    private readonly List<RankingRound> _rounds = new();
    public IReadOnlyCollection<RankingRound> Rounds => _rounds.AsReadOnly();

    public int MaxRounds => RoundsPerTeam * 2;

    private RankingGameSession() { } // EF Core

    public static RankingGameSession Create(IEnumerable<(string Name, string? Color, string? Icon)> teams, IEnumerable<Guid> categoryIds, int roundsPerTeam)
    {
        var teamsList = teams.ToList();
        var categories = categoryIds.ToList();

        if (teamsList.Count != 2)
            throw new InvalidOperationException("Ranking requires exactly 2 teams.");
        if (categories.Count == 0)
            throw new InvalidOperationException("A ranking session needs at least 1 category.");
        if (!AllowedRoundsPerTeam.Contains(roundsPerTeam))
            throw new InvalidOperationException($"عدد الجولات لكل فريق يجب أن يكون أحد القيم التالية: {string.Join(", ", AllowedRoundsPerTeam)}.");

        var session = new RankingGameSession
        {
            Id = Guid.NewGuid(),
            Status = RankingGameSessionStatus.Setup,
            RoundsPerTeam = roundsPerTeam,
            CreatedAt = DateTime.UtcNow
        };

        for (var i = 0; i < teamsList.Count; i++)
            session._teams.Add(RankingTeam.Create(session.Id, teamsList[i].Name, i, teamsList[i].Color, teamsList[i].Icon));

        session._categoryIds.AddRange(categories);
        return session;
    }

    public void Start()
    {
        if (Status != RankingGameSessionStatus.Setup)
            throw new InvalidOperationException("Only a session in Setup can be started.");
        Status = RankingGameSessionStatus.InProgress;
    }

    public void AttachOwner(Guid? userId) => UserId = userId;

    public RankingRound StartNextRound(Guid listId)
    {
        if (Status != RankingGameSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress.");
        if (_rounds.Count >= MaxRounds)
            throw new InvalidOperationException("All rounds have already been played.");
        if (_rounds.Any(r => r.Status == RankingRoundStatus.Pending))
            throw new InvalidOperationException("Submit the current round before starting the next one.");

        var roundNumber = _rounds.Count + 1;
        var turnOrder = (roundNumber - 1) % _teams.Count;
        var team = _teams.First(t => t.TurnOrder == turnOrder); // strict alternation, exactly 2 teams // strict alternation, exactly 2 teams
        var round = RankingRound.Create(Id, roundNumber, team.Id, listId);
        _rounds.Add(round);
        return round;
    }

    public void SubmitRound(Guid roundId, int pointsAwarded)
    {
        var round = _rounds.FirstOrDefault(r => r.Id == roundId)
            ?? throw new InvalidOperationException("Round does not belong to this session.");

        round.Submit(pointsAwarded);
        _teams.First(t => t.Id == round.TeamId).AddPoints(pointsAwarded);

        if (_rounds.Count == MaxRounds && _rounds.All(r => r.Status == RankingRoundStatus.Submitted))
            Complete();
    }

    public void Complete()
    {
        Status = RankingGameSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
