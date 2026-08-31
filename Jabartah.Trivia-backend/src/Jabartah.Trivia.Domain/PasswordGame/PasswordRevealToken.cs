using System.Security.Cryptography;

namespace Jabartah.Trivia.Domain.PasswordGame;

// A short-lived, single-use token embedded in a QR code on the shared screen. The
// clue-giver scans it with their own phone; the reveal page shows the word only to
// them, so nobody watching the shared screen ever sees it. WordId is denormalized
// here (rather than looked up via PasswordRoundId) because PasswordRound is not
// exposed as its own DbSet -- same reasoning as GameQuestionState.
public class PasswordRevealToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = default!;
    public Guid PasswordRoundId { get; private set; }
    public Guid PasswordWordId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    private PasswordRevealToken() { } // EF Core

    public static PasswordRevealToken Create(Guid roundId, Guid wordId, TimeSpan? ttl = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Token = GenerateToken(),
            PasswordRoundId = roundId,
            PasswordWordId = wordId,
            ExpiresAt = DateTime.UtcNow + (ttl ?? TimeSpan.FromMinutes(15))
        };

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))
            .Replace('+', '-').Replace('/', '_').TrimEnd('='); // url-safe, ~24 chars

    // Returns true only if this call is the one that consumed it.
    public bool TryConsume(out bool expired, out bool alreadyConsumed)
    {
        expired = DateTime.UtcNow > ExpiresAt;
        alreadyConsumed = ConsumedAt is not null;
        if (expired || alreadyConsumed) return false;

        ConsumedAt = DateTime.UtcNow;
        return true;
    }
}
