namespace Jabartah.Trivia.Application.Abstractions;

// Implemented in the Api project (wraps HttpContext/ClaimsPrincipal, an ASP.NET Core
// concern) so Application code can read "who is calling" without referencing ASP.NET Core.
public interface ICurrentUserAccessor
{
    Guid? UserId { get; }   // null when anonymous/unauthenticated
}
