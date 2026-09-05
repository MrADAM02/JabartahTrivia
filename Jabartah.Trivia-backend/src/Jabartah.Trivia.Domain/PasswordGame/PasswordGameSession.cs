namespace Jabartah.Trivia.Domain.PasswordGame;

public enum PasswordGameSessionStatus
{
    Setup = 0,
    InProgress = 1,
    Completed = 2
}

public enum PasswordRoundOutcome
{
    Pending = 0,
    Correct = 1,
    Skipped = 2
}

// Tracks one played round: which team was guessing, which word, and the outcome.
public class PasswordRound
{
    public Guid Id { get; private set; }
    public Guid PasswordGameSessionId { get; private set; }
    public int RoundNumber { get; private set; }
    public Guid TeamId { get; private set; }          // clue-giver's team
    public Guid PasswordWordId { get; private set; }
    public PasswordRoundOutcome Outcome { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private PasswordRound() { } // EF Core

    public static PasswordRound Create(Guid sessionId, int roundNumber, Guid teamId, Guid wordId) =>
        new()
        {
            Id = Guid.NewGuid(),
            PasswordGameSessionId = sessionId,
            RoundNumber = roundNumber,
            TeamId = teamId,
            PasswordWordId = wordId,
            Outcome = PasswordRoundOutcome.Pending,
            CreatedAt = DateTime.UtcNow
        };

    public void Resolve(PasswordRoundOutcome outcome)
    {
        if (Outcome != PasswordRoundOutcome.Pending)
            throw new InvalidOperationException("This round was already resolved.");
        Outcome = outcome;
        ResolvedAt = DateTime.UtcNow;
    }
}

// Aggregate root: two teams alternate turns guessing secret words via a clue-giver.
// See CLAUDE.md for the QR-reveal UX this pairs with (PasswordRevealToken).
public class PasswordGameSession
{
    public static readonly int[] AllowedRoundsPerTeam = [3, 5, 7];
    public const int PointsPerWord = 1;

    public int RoundsPerTeam { get; private set; }

    public Guid Id { get; private set; }
    public PasswordGameSessionStatus Status { get; private set; }
    public Guid? UserId { get; private set; }   // owner, if created while logged in; null for guest play
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<PasswordTeam> _teams = new();
    public IReadOnlyCollection<PasswordTeam> Teams => _teams.AsReadOnly();

    private readonly List<Guid> _categoryIds = new();
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    private readonly List<PasswordRound> _rounds = new();
    public IReadOnlyCollection<PasswordRound> Rounds => _rounds.AsReadOnly();

    public int MaxRounds => RoundsPerTeam * 2;

    private PasswordGameSession() { } // EF Core

    public static PasswordGameSession Create(IEnumerable<(string Name, string? Color, string? Icon)> teams, IEnumerable<Guid> categoryIds, int roundsPerTeam)
    {
        var teamsList = teams.ToList();
        var categories = categoryIds.ToList();

        if (teamsList.Count != 2)
            throw new InvalidOperationException("Password requires exactly 2 teams.");
        if (categories.Count == 0)
            throw new InvalidOperationException("A password session needs at least 1 category.");
        if (!AllowedRoundsPerTeam.Contains(roundsPerTeam))
            throw new InvalidOperationException($"عدد الجولات لكل فريق يجب أن يكون أحد القيم التالية: {string.Join(", ", AllowedRoundsPerTeam)}.");

        var session = new PasswordGameSession
        {
            Id = Guid.NewGuid(),
            Status = PasswordGameSessionStatus.Setup,
            RoundsPerTeam = roundsPerTeam,
            CreatedAt = DateTime.UtcNow
        };

        for (var i = 0; i < teamsList.Count; i++)
            session._teams.Add(PasswordTeam.Create(session.Id, teamsList[i].Name, i, teamsList[i].Color, teamsList[i].Icon));

        session._categoryIds.AddRange(categories);
        return session;
    }

    public void Start()
    {
        if (Status != PasswordGameSessionStatus.Setup)
            throw new InvalidOperationException("Only a session in Setup can be started.");
        Status = PasswordGameSessionStatus.InProgress;
    }

    public void AttachOwner(Guid? userId) => UserId = userId;

    public PasswordRound StartNextRound(Guid wordId)
    {
        if (Status != PasswordGameSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress.");
        if (_rounds.Count >= MaxRounds)
            throw new InvalidOperationException("All rounds have already been played.");
        if (_rounds.Any(r => r.Outcome == PasswordRoundOutcome.Pending))
            throw new InvalidOperationException("Resolve the current round before starting the next one.");

        var roundNumber = _rounds.Count + 1;
        var turnOrder = (roundNumber - 1) % _teams.Count;
        var team = _teams.First(t => t.TurnOrder == turnOrder); // strict alternation, exactly 2 teams
        var round = PasswordRound.Create(Id, roundNumber, team.Id, wordId);
        _rounds.Add(round);
        return round;
    }

    // Purely a one-time-per-game flag; the actual +15s is a frontend-only visual
    // countdown adjustment (the round timer itself is never enforced server-side).
    public void UseExtraTime(Guid teamId)
    {
        var team = _teams.FirstOrDefault(t => t.Id == teamId)
            ?? throw new InvalidOperationException("Team does not belong to this session.");
        team.UseExtraTime();
    }

    public void ResolveRound(Guid roundId, PasswordRoundOutcome outcome)
    {
        var round = _rounds.FirstOrDefault(r => r.Id == roundId)
            ?? throw new InvalidOperationException("Round does not belong to this session.");

        round.Resolve(outcome);

        if (outcome == PasswordRoundOutcome.Correct)
            _teams.First(t => t.Id == round.TeamId).AddPoints(PointsPerWord);

        if (_rounds.Count == MaxRounds && _rounds.All(r => r.Outcome != PasswordRoundOutcome.Pending))
            Complete();
    }

    public void Complete()
    {
        if (Status == PasswordGameSessionStatus.Completed) return;
        Status = PasswordGameSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
