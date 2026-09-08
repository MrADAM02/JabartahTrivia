using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.CreateTop100Category;
using Jabartah.Trivia.Application.Categories.DeleteTop100Category;
using Jabartah.Trivia.Application.Categories.ListTop100CategoriesForAdmin;
using Jabartah.Trivia.Application.Categories.UpdateTop100Category;
using Jabartah.Trivia.Application.Top100Lists.CreateTop100List;
using Jabartah.Trivia.Application.Top100Lists.CreateTop100ListItem;
using Jabartah.Trivia.Application.Top100Lists.DeleteTop100List;
using Jabartah.Trivia.Application.Top100Lists.DeleteTop100ListItem;
using Jabartah.Trivia.Application.Top100Lists.GetTop100ListForAdmin;
using Jabartah.Trivia.Application.Top100Lists.ListTop100ListsByCategory;
using Jabartah.Trivia.Application.Top100Lists.UpdateTop100List;
using Jabartah.Trivia.Application.Top100Lists.UpdateTop100ListItem;

namespace Jabartah.Trivia.Api.Endpoints;

public static class AdminTop100Endpoints
{
    public static void MapAdminTop100Endpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/top100").WithTags("AdminTop100")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListTop100CategoriesForAdminQuery(), ct)));

        group.MapPost("/categories", async (CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateTop100CategoryCommand(body.Name, body.Icon, body.Description), ct)));

        group.MapPut("/categories/{id:guid}", async (Guid id, CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateTop100CategoryCommand(id, body.Name, body.Icon, body.Description), ct)));

        group.MapDelete("/categories/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteTop100CategoryCommand(id), ct)));

        group.MapGet("/categories/{id:guid}/lists", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListTop100ListsByCategoryQuery(id), ct)));

        group.MapPost("/categories/{id:guid}/lists", async (Guid id, ListRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateTop100ListCommand(id, body.Title), ct)));

        group.MapGet("/lists/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetTop100ListForAdminQuery(id), ct)));

        group.MapPut("/lists/{id:guid}", async (Guid id, ListRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateTop100ListCommand(id, body.Title), ct)));

        group.MapDelete("/lists/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteTop100ListCommand(id), ct)));

        group.MapPost("/lists/{id:guid}/items", async (Guid id, ItemRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateTop100ListItemCommand(id, body.Label, body.Position, body.AlternateSpellings ?? []), ct)));

        group.MapPut("/items/{id:guid}", async (Guid id, ItemRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateTop100ListItemCommand(id, body.Label, body.Position, body.AlternateSpellings ?? []), ct)));

        group.MapDelete("/items/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteTop100ListItemCommand(id), ct)));
    }

    public record CategoryRequest(string Name, string? Icon, string? Description);
    public record ListRequest(string Title);
    public record ItemRequest(string Label, int Position, List<string>? AlternateSpellings);
}
