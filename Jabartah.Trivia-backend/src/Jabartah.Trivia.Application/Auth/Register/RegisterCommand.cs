using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Auth.Register;

public record RegisterCommand(string Name, string Email, string Password) : ICommand<AuthResult>;

public record AuthResult(string Token, Guid UserId, string Name, string Email);

public class RegisterHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
            throw new InvalidOperationException("هذا البريد الإلكتروني مستخدم بالفعل.");

        var passwordHash = passwordHasher.Hash(command.Password);
        var user = User.Create(command.Name, command.Email, passwordHash);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = jwtTokenGenerator.Generate(user.Id, user.Email, user.Name);
        return new AuthResult(token, user.Id, user.Name, user.Email);
    }
}
