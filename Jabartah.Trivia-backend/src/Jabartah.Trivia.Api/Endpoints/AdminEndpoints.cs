using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Admin.GetDashboardStats;
using Jabartah.Trivia.Application.Categories.CreateCategory;
using Jabartah.Trivia.Application.Categories.DeleteCategory;
using Jabartah.Trivia.Application.Categories.ListCategoriesForAdmin;
using Jabartah.Trivia.Application.Categories.UpdateCategory;
using Jabartah.Trivia.Application.Questions.CreateQuestion;
using Jabartah.Trivia.Application.Questions.DeleteQuestion;
using Jabartah.Trivia.Application.Questions.ListQuestionsByCategory;
using Jabartah.Trivia.Application.Questions.UpdateQuestion;

namespace Jabartah.Trivia.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/stats", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetDashboardStatsQuery(), ct)));

        group.MapGet("/categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListCategoriesForAdminQuery(), ct)));

        group.MapPost("/categories", async (CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateCategoryCommand(body.Name, body.Icon), ct)));

        group.MapPut("/categories/{id:guid}", async (Guid id, CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateCategoryCommand(id, body.Name, body.Icon), ct)));

        group.MapDelete("/categories/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteCategoryCommand(id), ct)));

        group.MapGet("/categories/{id:guid}/questions", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListQuestionsByCategoryQuery(id), ct)));

        group.MapPost("/categories/{id:guid}/questions", async (
                Guid id, QuestionRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(
                new CreateQuestionCommand(id, body.PointValue, body.Prompt, body.Answer, body.MediaUrl), ct)));

        group.MapPut("/questions/{id:guid}", async (Guid id, QuestionRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(
                new UpdateQuestionCommand(id, body.PointValue, body.Prompt, body.Answer, body.MediaUrl), ct)));

        group.MapDelete("/questions/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteQuestionCommand(id), ct)));
    }

    public record CategoryRequest(string Name, string? Icon);
    public record QuestionRequest(int PointValue, string Prompt, string Answer, string? MediaUrl);
}
