using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Auth.DeleteAccount;
using Jabartah.Trivia.Application.Auth.GetAccount;
using Jabartah.Trivia.Application.Auth.Login;
using Jabartah.Trivia.Application.Auth.Register;

namespace Jabartah.Trivia.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        auth.MapPost("/register", async (RegisterCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(command, ct)));

        auth.MapPost("/login", async (LoginCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(command, ct)));

        var account = app.MapGroup("/api/account").WithTags("Account").RequireAuthorization();

        account.MapGet("/", async (ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetAccountQuery(currentUser.UserId!.Value), ct)));

        account.MapDelete("/", async (ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteAccountCommand(currentUser.UserId!.Value), ct)));
    }
}
