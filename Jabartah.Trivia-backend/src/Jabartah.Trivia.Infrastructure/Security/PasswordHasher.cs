using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Jabartah.Trivia.Infrastructure.Security;

// Wraps ASP.NET Core's PasswordHasher<TUser> -- a single dependency-free class (PBKDF2-HMAC-SHA256,
// versioned for future upgrades), not the full Identity framework (no UserManager/roles/cookies).
public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(default!, password);

    public bool Verify(string hash, string password) =>
        _inner.VerifyHashedPassword(default!, hash, password) != PasswordVerificationResult.Failed;
}
