using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.ListCategories;
using Jabartah.Trivia.Application.GameSessions.AwardPoints;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Jabartah.Trivia.Application.GameSessions.GetBoard;
using Jabartah.Trivia.Application.GameSessions.SelectQuestion;
using Microsoft.Extensions.DependencyInjection;

namespace Jabartah.Trivia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Explicit registration is fine at this scale (4 handlers). Once this list
        // grows past ~15-20, swap to Scrutor assembly scanning instead of hand-adding each one.
        services.AddScoped<ICommandHandler<CreateGameSessionCommand, CreateGameSessionResult>, CreateGameSessionHandler>();
        services.AddScoped<IQueryHandler<GetBoardQuery, BoardDto>, GetBoardHandler>();
        services.AddScoped<ICommandHandler<SelectQuestionCommand, SelectQuestionResult>, SelectQuestionHandler>();
        services.AddScoped<ICommandHandler<AwardPointsCommand, AwardPointsResult>, AwardPointsHandler>();
        services.AddScoped<IQueryHandler<ListCategoriesQuery, List<CategoryDto>>, ListCategoriesHandler>();

        return services;
    }
}
