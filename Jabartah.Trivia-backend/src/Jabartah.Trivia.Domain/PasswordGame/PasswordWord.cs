namespace Jabartah.Trivia.Domain.PasswordGame;

public class PasswordWord
{
    public Guid Id { get; private set; }
    public Guid PasswordCategoryId { get; private set; }
    public string Word { get; private set; } = default!;

    private PasswordWord() { } // EF Core

    public static PasswordWord Create(Guid passwordCategoryId, string word)
    {
        if (string.IsNullOrWhiteSpace(word)) throw new ArgumentException("Word is required.", nameof(word));

        return new PasswordWord
        {
            Id = Guid.NewGuid(),
            PasswordCategoryId = passwordCategoryId,
            Word = word
        };
    }
}
