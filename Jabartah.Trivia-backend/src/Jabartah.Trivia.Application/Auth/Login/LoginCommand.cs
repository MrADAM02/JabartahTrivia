using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Auth.Register;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Auth.Login;

public record LoginCommand(string Email, string Password) : ICommand<AuthResult>;

public class LoginHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        // Same error for "no such user" and "wrong password" -- avoids leaking which emails are registered.
        if (user is null || !passwordHasher.Verify(user.PasswordHash, command.Password))
            throw new KeyNotFoundException("بيانات الدخول غير صحيحة.");

        var token = jwtTokenGenerator.Generate(user.Id, user.Email, user.Name);
        return new AuthResult(token, user.Id, user.Name, user.Email);
    }
}
