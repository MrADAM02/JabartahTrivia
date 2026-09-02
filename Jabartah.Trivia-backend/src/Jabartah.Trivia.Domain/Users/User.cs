namespace Jabartah.Trivia.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;   // normalized lowercase, unique
    public string PasswordHash { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private User() { } // EF Core

    public static User Create(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("الاسم مطلوب.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("البريد الإلكتروني مطلوب.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }
}
