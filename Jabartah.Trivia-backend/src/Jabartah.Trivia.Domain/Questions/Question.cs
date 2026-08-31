namespace Jabartah.Trivia.Domain.Questions;

public class Question
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public int PointValue { get; private set; }   // 100 / 200 / 300 / 400 / 500
    public string Prompt { get; private set; } = default!;  // Arabic question text
    public string Answer { get; private set; } = default!;  // Arabic answer text
    public string? MediaUrl { get; private set; }           // optional image/audio/video

    private Question() { } // EF Core

    public static Question Create(Guid categoryId, int pointValue, string prompt, string answer, string? mediaUrl = null)
    {
        if (pointValue <= 0) throw new ArgumentException("Point value must be positive.", nameof(pointValue));
        if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException("Prompt is required.", nameof(prompt));
        if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Answer is required.", nameof(answer));

        return new Question
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            PointValue = pointValue,
            Prompt = prompt,
            Answer = answer,
            MediaUrl = mediaUrl
        };
    }
}
