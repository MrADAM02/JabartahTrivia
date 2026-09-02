namespace Jabartah.Trivia.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string Generate(Guid userId, string email, string name);
}
