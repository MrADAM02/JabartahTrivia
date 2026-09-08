using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.CreatePasswordCategory;
using Jabartah.Trivia.Application.Categories.DeletePasswordCategory;
using Jabartah.Trivia.Application.Categories.ListPasswordCategoriesForAdmin;
using Jabartah.Trivia.Application.Categories.UpdatePasswordCategory;
using Jabartah.Trivia.Application.Words.CreateWord;
using Jabartah.Trivia.Application.Words.DeleteWord;
using Jabartah.Trivia.Application.Words.ListWordsByCategory;
using Jabartah.Trivia.Application.Words.UpdateWord;

namespace Jabartah.Trivia.Api.Endpoints;

public static class AdminPasswordEndpoints
{
    public static void MapAdminPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/password").WithTags("AdminPassword")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListPasswordCategoriesForAdminQuery(), ct)));

        group.MapPost("/categories", async (CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreatePasswordCategoryCommand(body.Name, body.Icon), ct)));

        group.MapPut("/categories/{id:guid}", async (Guid id, CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdatePasswordCategoryCommand(id, body.Name, body.Icon), ct)));

        group.MapDelete("/categories/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeletePasswordCategoryCommand(id), ct)));

        group.MapGet("/categories/{id:guid}/words", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListWordsByCategoryQuery(id), ct)));

        group.MapPost("/categories/{id:guid}/words", async (Guid id, WordRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateWordCommand(id, body.Word), ct)));

        group.MapPut("/words/{id:guid}", async (Guid id, WordRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateWordCommand(id, body.Word), ct)));

        group.MapDelete("/words/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteWordCommand(id), ct)));
    }

    public record CategoryRequest(string Name, string? Icon);
    public record WordRequest(string Word);
}
