namespace Jabartah.Trivia.Domain.Users;

// Role changes are an operator action (a one-time manual DB update), not a
// user-facing feature -- there is deliberately no PromoteToAdmin() method.
public enum UserRole
{
    User = 0,
    Admin = 1
}

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;   // normalized lowercase, unique
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
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
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
    }
}
